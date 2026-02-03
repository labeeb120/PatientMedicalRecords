using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;
using System.Text.Json;

namespace PatientMedicalRecords.Services
{
    public interface IDrugInteractionService
    {
        Task<DrugInteractionCheckResponse> CheckDrugInteractionsAsync(DrugInteractionCheckRequest request);
        Task<ServiceResult> CreatePrescriptionAsync(PrescriptionCreateRequest request);
        Task<List<DrugInteractionWarning>> GetKnownInteractionsAsync(List<int> ingredientIds);
        Task<ServiceResult> BulkImportDrugsAsync(List<DrugImportDto> drugs);
        Task<ServiceResult> BulkImportInteractionsAsync(List<InteractionImportDto> interactions);
        //Task<List<DrugSuggestionDto>> GetDrugSuggestionsAsync(string partialName);
    }

    

    // فحص التفاعلات لمريض بناءً على قائمة الأدوية الجديدة
    public class DrugInteractionService : IDrugInteractionService
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly ILogger<DrugInteractionService> _logger;

        public DrugInteractionService(MedicalRecordsDbContext context, ILogger<DrugInteractionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string Normalize(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();

        //**************************************
        public async Task<DrugInteractionCheckResponse> CheckDrugInteractionsAsync(DrugInteractionCheckRequest request)
        {
            try
            {
                // 1) الأدوية الحالية للمريض (معرفات الأدوية المخزنة)
                var currentDrugIds = await GetPatientCurrentDrugIdsAsync(request.PatientId);

                // 2) الأدوية الجديدة المقترحة
                var allDrugIds = new HashSet<int>(currentDrugIds);
                var newDrugIdsFromRequest = request.DrugIds ?? new List<int>();

                // إضافة المعرفات المرسلة مباشرة
                foreach (var id in newDrugIdsFromRequest)
                {
                    allDrugIds.Add(id);
                }

                var newMedicationNames = (request.Medications ?? new List<string>()).Select(n => Normalize(n)).ToHashSet();

                // نبحث عن معرفات الأدوية الجديدة بناءً على الأسماء إذا تم إرسالها
                if (newMedicationNames.Any())
                {
                    var newDrugsFromNames = await _context.Drugs
                        .Where(d => newMedicationNames.Contains(d.NormalizedName!))
                        .Select(d => d.Id)
                        .ToListAsync();

                    foreach (var id in newDrugsFromNames)
                    {
                        allDrugIds.Add(id);
                    }
                }

                // إذا لم يتم العثور على أدوية جديدة أو حالية
                if (!allDrugIds.Any())
                {
                    return new DrugInteractionCheckResponse
                    {
                        Success = true,
                        Message = "لم يتم العثور على أي أدوية مسجلة للتحقق من التفاعلات",
                        Warnings = new List<DrugInteractionWarning>(),
                        HasInteractions = false
                    };
                }


                // 3) جمع معرفات المركبات النشطة (Ingredients) لجميع الأدوية
                var ingredientIds = await _context.DrugIngredients
                    .Where(di => allDrugIds.Contains(di.DrugId))
                    .Select(di => di.IngredientId)
                    .Distinct()
                    .ToListAsync();
                //*****************************
                var comparisonSteps = new List<DrugComparisonStep>();

                for (int i = 0; i < ingredientIds.Count; i++)
                {
                    for (int j = i + 1; j < ingredientIds.Count; j++)
                    {
                        var a = ingredientIds[i];
                        var b = ingredientIds[j];

                        var found = await _context.DrugInteractions.AnyAsync(x =>
                            (x.IngredientAId == a && x.IngredientBId == b) ||
                            (x.IngredientAId == b && x.IngredientBId == a));

                        comparisonSteps.Add(new DrugComparisonStep
                        {
                            IngredientA = a.ToString(), // أو اسم المركب
                            IngredientB = b.ToString(),
                            InteractionFound = found
                        });
                    }
                }

                //*****************************


                // 4) فحص التفاعلات
                var warnings = new List<DrugInteractionWarning>();
                if (ingredientIds.Count >= 2)
                {
                    // جلب جميع التفاعلات حيث IngredientA و IngredientB كلاهما ضمن القائمة
                    var interactions = await _context.DrugInteractions // *تم التعديل: DrugInteractions بدلاً من IngredientInteractions*
                        .Where(ii => ingredientIds.Contains(ii.IngredientAId) && ingredientIds.Contains(ii.IngredientBId))
                        .Include(ii => ii.IngredientA)
                        .Include(ii => ii.IngredientB)
                        .ToListAsync();

                    // 5) ربط التفاعلات بأسماء الأدوية لتوضيح التحذير للصيدلي
                    var drugs = await _context.Drugs
                        .Include(d => d.DrugIngredients)
                        .Where(d => allDrugIds.Contains(d.Id))
                        .ToListAsync();

                    foreach (var inter in interactions)
                    {
                        // البحث عن اسم الدواء (التجاري أو العلمي) الذي يحتوي على كل مركب نشط
                        var drugNamesA = drugs
                            .Where(d => d.DrugIngredients.Any(di => di.IngredientId == inter.IngredientAId))
                            .Select(d => d.BrandName ?? d.ScientificName)
                            .Distinct()
                            .ToList();

                        var drugNamesB = drugs
                            .Where(d => d.DrugIngredients.Any(di => di.IngredientId == inter.IngredientBId))
                            .Select(d => d.BrandName ?? d.ScientificName)
                            .Distinct()
                            .ToList();

                        // اختيار اسمين لتمثيل التفاعل (يفضل عرض جميع الأسماء المتفاعلة)
                        var medNameA = string.Join(", ", drugNamesA);
                        var medNameB = string.Join(", ", drugNamesB);


                        warnings.Add(new DrugInteractionWarning
                        {
                            Medication1 = medNameA,
                            Medication2 = medNameB,
                            Severity = inter.Severity,
                            Description = inter.Description,
                            Recommendation = inter.Recommendation
                        });
                    }
                }

                // فلترة التحذيرات بحيث تكون ذات صلة بالطلب الحالي (الأدوية الجديدة)
                // نعتبر التحذير ذا صلة إذا كان أحد الأدوية المتفاعلة هو دواء جديد تم إرساله في الطلب (بواسطة ID أو الاسم)
                var newDrugsData = await _context.Drugs
                    .Where(d => allDrugIds.Contains(d.Id) && !currentDrugIds.Contains(d.Id))
                    .Select(d => new { d.BrandName, d.ScientificName, d.NormalizedName })
                    .ToListAsync();

                var newDrugNames = newDrugsData
                    .SelectMany(d => new[] { Normalize(d.BrandName ?? ""), Normalize(d.ScientificName ?? ""), d.NormalizedName ?? "" })
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToHashSet();

                var relevantWarnings = warnings.Where(w =>
                    newDrugNames.Any(n => Normalize(w.Medication1).Contains(n) || Normalize(w.Medication2).Contains(n)) ||
                    newMedicationNames.Any(n => Normalize(w.Medication1).Contains(n) || Normalize(w.Medication2).Contains(n))
                ).ToList();


                //return new DrugInteractionCheckResponse
                //{
                    return new DrugInteractionCheckResponse
                    {
                        Success = true,
                        Message = relevantWarnings.Any()
                                        ? "تم العثور على تفاعلات دوائية"
                                        : "تم فحص جميع الأدوية ولم يتم العثور على تفاعلات",
                        Warnings = relevantWarnings,
                        HasInteractions = relevantWarnings.Any(),
                        ComparisonSteps = comparisonSteps
                    };

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking drug interactions for patient {PatientId}", request.PatientId);
                return new DrugInteractionCheckResponse
                {
                    Success = false,
                    Message = "حدث خطأ أثناء فحص التفاعلات الدوائية",
                    Warnings = new List<DrugInteractionWarning>(),
                    HasInteractions = false
                };
            }
        }

       

        public async Task<List<DrugInteractionWarning>> GetKnownInteractionsAsync(List<string> medications)
        {
            // يمكن الاحتفاظ بهذه للدعم الخلفي، لكن من الأفضل استخدام CheckDrugInteractionsAsync مباشرة
            throw new NotImplementedException("Use CheckDrugInteractionsAsync which uses Ingredients.");
        }





        //******************************************
        public async Task<bool> AddDrugInteractionAsync(string ingredient1, string ingredient2, string severity, string description, string recommendation)
        {
            try
            {
                var n1 = Normalize(ingredient1);
                var n2 = Normalize(ingredient2);

                var ing1 = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == n1)
                               ?? new Ingredient { Name = ingredient1, NormalizedName = n1 };

                var ing2 = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == n2)
                               ?? new Ingredient { Name = ingredient2, NormalizedName = n2 };

                if (ing1.Id == 0) _context.Ingredients.Add(ing1);
                if (ing2.Id == 0) _context.Ingredients.Add(ing2);

                await _context.SaveChangesAsync();

                var a = Math.Min(ing1.Id, ing2.Id);
                var b = Math.Max(ing1.Id, ing2.Id);

                var exists = await _context.DrugInteractions // *تم التعديل*
                    .AnyAsync(ii => ii.IngredientAId == a && ii.IngredientBId == b);

                if (exists) return false;

                var interaction = new DrugInteraction // *تم التعديل*
                {
                    IngredientAId = a,
                    IngredientBId = b,
                    Severity = severity,
                    Description = description,
                    Recommendation = recommendation
                };

                _context.DrugInteractions.Add(interaction); // *تم التعديل*
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ingredient interaction {0} - {1}", ingredient1, ingredient2);
                return false;
            }
        }



      



        public async Task<List<string>> GetMedicationSuggestionsAsync(string partialName)
        {
            var normalized = Normalize(partialName);

            // البحث عن أسماء الأدوية (الاسم التجاري أو العلمي)
            var meds = await _context.Drugs // *تم التعديل: Drugs بدلاً من Medications*
                .Where(m => m.NormalizedName!.Contains(normalized))
                .OrderBy(m => m.BrandName) // يفضل الترتيب بالاسم التجاري
                .Take(20)
                .Select(m => m.BrandName ?? m.ScientificName) // عرض الاسم التجاري أو العلمي
                .ToListAsync();

            // إذا لم يتم العثور على أدوية، قد نقترح أسماء المكونات الفعالة
            if (!meds.Any())
            {
                meds = await _context.Ingredients
                    .Where(i => i.NormalizedName.Contains(normalized))
                    .Take(20)
                    .Select(i => i.Name)
                    .ToListAsync();
            }

            return meds;
        }
    


        private async Task<List<int>> GetPatientCurrentDrugIdsAsync(int patientId)
        {
            try
            {
                var currentDrugIds = await _context.PrescriptionItems
                    .Where(pi => pi.Prescription.PatientId == patientId &&
                                 pi.Prescription.Status == PrescriptionStatus.Dispensed &&
                                 pi.IsDispensed)
                    .Select(pi => pi.DrugId) // نستخدم DrugId بدلاً من MedicationName
                    .Distinct()
                    .ToListAsync();

                return currentDrugIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current drug IDs for patient {PatientId}", patientId);
                return new List<int>();
            }
        }


       

        public Task<List<DrugInteractionWarning>> GetKnownInteractionsAsync(List<int> ingredientIds)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> CreatePrescriptionAsync(PrescriptionCreateRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<List<DrugSuggestionDto>> GetDrugSuggestionsAsync(string partialName)
        {
            throw new NotImplementedException();
        }

        // 26-01-2026: Bulk Import Implementation
        public async Task<ServiceResult> BulkImportDrugsAsync(List<DrugImportDto> drugs)
        {
            try
            {
                int addedCount = 0;
                foreach (var d in drugs)
                {
                    var normalized = Normalize(d.ScientificName);
                    var existingDrug = await _context.Drugs
                        .Include(dr => dr.DrugIngredients)
                        .FirstOrDefaultAsync(dr => dr.NormalizedName == normalized || dr.ScientificName == d.ScientificName);

                    if (existingDrug == null)
                    {
                        existingDrug = new Drug
                        {
                            ScientificName = d.ScientificName,
                            BrandName = d.BrandName,
                            ChemicalName = d.ChemicalName,
                            Manufacturer = d.Manufacturer,
                            NormalizedName = normalized,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Drugs.Add(existingDrug);
                        await _context.SaveChangesAsync();
                        addedCount++;
                    }

                    // Handle Ingredients
                    foreach (var ingName in d.Ingredients)
                    {
                        var normalizedIng = Normalize(ingName);
                        var ingredient = await _context.Ingredients
                            .FirstOrDefaultAsync(i => i.NormalizedName == normalizedIng || i.Name == ingName);

                        if (ingredient == null)
                        {
                            ingredient = new Ingredient
                            {
                                Name = ingName,
                                NormalizedName = normalizedIng
                            };
                            _context.Ingredients.Add(ingredient);
                            await _context.SaveChangesAsync();
                        }

                        // Link Drug to Ingredient
                        var exists = existingDrug.DrugIngredients.Any(di => di.IngredientId == ingredient.Id);
                        if (!exists)
                        {
                            _context.DrugIngredients.Add(new DrugIngredient
                            {
                                DrugId = existingDrug.Id,
                                IngredientId = ingredient.Id
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Ok($"تم استيراد {addedCount} أدوية جديدة بنجاح وتحديث المكونات.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk drug import");
                return ServiceResult.Fail("حدث خطأ أثناء استيراد الأدوية.");
            }
        }

        public async Task<ServiceResult> BulkImportInteractionsAsync(List<InteractionImportDto> interactions)
        {
            try
            {
                int addedCount = 0;
                foreach (var inter in interactions)
                {
                    var normA = Normalize(inter.IngredientAName);
                    var normB = Normalize(inter.IngredientBName);

                    var ingA = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == normA);
                    var ingB = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == normB);

                    if (ingA == null || ingB == null) continue;

                    var idA = Math.Min(ingA.Id, ingB.Id);
                    var idB = Math.Max(ingA.Id, ingB.Id);

                    var existing = await _context.DrugInteractions
                        .FirstOrDefaultAsync(di => di.IngredientAId == idA && di.IngredientBId == idB);

                    if (existing == null)
                    {
                        _context.DrugInteractions.Add(new DrugInteraction
                        {
                            IngredientAId = idA,
                            IngredientBId = idB,
                            Severity = inter.Severity,
                            Description = inter.Description,
                            Recommendation = inter.Recommendation,
                            CreatedAt = DateTime.UtcNow
                        });
                        addedCount++;
                    }
                    else
                    {
                        existing.Severity = inter.Severity;
                        existing.Description = inter.Description;
                        existing.Recommendation = inter.Recommendation;
                    }
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Ok($"تم استيراد/تحديث {addedCount} تفاعلات دوائية بنجاح.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk interactions import");
                return ServiceResult.Fail("حدث خطأ أثناء استيراد التفاعلات.");
            }
        }
    }
}






//public interface IDrugInteractionService
//{
//    Task<DrugInteractionCheckResponse> CheckDrugInteractionsAsync(DrugInteractionCheckRequest request);
//    Task<List<DrugInteractionWarning>> GetKnownInteractionsAsync(List<string> medications);
//    Task<bool> AddDrugInteractionAsync(string medication1, string medication2, string severity, string description, string recommendation);
//    Task<List<string>> GetMedicationSuggestionsAsync(string partialName);
//}

//***********************************************************************************
//public class DrugInteractionService : IDrugInteractionService
//{
//    private readonly MedicalRecordsDbContext _context;
//    private readonly ILogger<DrugInteractionService> _logger;

//    public DrugInteractionService(MedicalRecordsDbContext context, ILogger<DrugInteractionService> logger)
//    {
//        _context = context;
//        _logger = logger;
//    }

//    private string Normalize(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();

//    public async Task<DrugInteractionCheckResponse> CheckDrugInteractionsAsync(DrugInteractionCheckRequest request)
//    {
//        try
//        {
//            // 1) patient current medications (names) -> get their ingredients
//            var currentMedications = await GetPatientCurrentMedicationsAsync(request.PatientId);

//            // 2) combine medication names (original strings)
//            var allMedicationNames = currentMedications
//                 .Union(request.Medications)
//                 .Distinct(StringComparer.OrdinalIgnoreCase)
//                 .ToList();

//            // 3) map medication names -> medication records (if exist)
//            var normalizedNames = allMedicationNames.Select(n => Normalize(n)).ToList();

//            var medications = await _context.Medications
//                .Where(m => normalizedNames.Contains(m.NormalizedName))
//                .Include(m => m.MedicationIngredients)
//                    .ThenInclude(mi => mi.Ingredient)
//                .ToListAsync();

//            // 4) for any medication name not mapped, try fuzzy match by startingWith or suggest to admin
//            var mappedNames = medications.Select(m => m.NormalizedName).ToHashSet();
//            var unknownMedicationNames = allMedicationNames
//                .Where(n => !mappedNames.Contains(Normalize(n)))
//                .ToList();

//            // (Optional) you may log or return suggestions for unknown meds so admin can map them

//            // 5) collect ingredient IDs
//            var ingredientIds = medications
//                .SelectMany(m => m.MedicationIngredients.Select(mi => mi.IngredientId))
//                .Distinct()
//                .ToList();

//            // 6) If there are unknown meds, you could try to lookup by ingredient name directly if request includes them (not in current DTO)

//            // 7) fetch interactions for any pair of ingredients
//            var warnings = new List<DrugInteractionWarning>();
//            if (ingredientIds.Count >= 2)
//            {
//                // get all interactions where A in ids and B in ids
//                // ensuring we check both orders: we stored A<B invariantly
//                var interactions = await _context.IngredientInteractions
//                    .Where(ii => ingredientIds.Contains(ii.IngredientAId) && ingredientIds.Contains(ii.IngredientBId))
//                    .Include(ii => ii.IngredientA)
//                    .Include(ii => ii.IngredientB)
//                    .ToListAsync();

//                // Map interactions to medication names: for user clarity, we map ingredient -> medication name(s) involved
//                foreach (var inter in interactions)
//                {
//                    // find medication names that contain these ingredients
//                    var medsA = medications
//                        .Where(m => m.MedicationIngredients.Any(mi => mi.IngredientId == inter.IngredientAId))
//                        .Select(m => m.Name)
//                        .Distinct()
//                        .ToList();

//                    var medsB = medications
//                        .Where(m => m.MedicationIngredients.Any(mi => mi.IngredientId == inter.IngredientBId))
//                        .Select(m => m.Name)
//                        .Distinct()
//                        .ToList();

//                    // for clarity, report one representative med name from each side (or list all)
//                    var medNameA = medsA.FirstOrDefault() ?? inter.IngredientA.Name;
//                    var medNameB = medsB.FirstOrDefault() ?? inter.IngredientB.Name;

//                    // Only show warnings that involve at least one medication from the incoming request (new medications)
//                    bool involvesNewMed = request.Medications
//                        .Select(n => Normalize(n))
//                        .Intersect(medications.Select(m => m.NormalizedName))
//                        .Any();

//                    warnings.Add(new DrugInteractionWarning
//                    {
//                        Medication1 = medNameA,
//                        Medication2 = medNameB,
//                        Severity = inter.Severity,
//                        Description = inter.Description,
//                        Recommendation = inter.Recommendation
//                    });
//                }
//            }

//            var relevantWarnings = warnings
//                // optionally filter to only include those where at least one of the meds is in request.Medications
//                .Where(w =>
//                    request.Medications.Any(r => Normalize(r) == Normalize(w.Medication1)) ||
//                    request.Medications.Any(r => Normalize(r) == Normalize(w.Medication2)))
//                .ToList();

//            return new DrugInteractionCheckResponse
//            {
//                Success = true,
//                Message = relevantWarnings.Any() ? "تم العثور على تفاعلات دوائية" : "لا توجد تفاعلات دوائية",
//                Warnings = relevantWarnings,
//                HasInteractions = relevantWarnings.Any()
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error checking drug interactions for patient {PatientId}", request.PatientId);
//            return new DrugInteractionCheckResponse
//            {
//                Success = false,
//                Message = "حدث خطأ أثناء فحص التفاعلات الدوائية",
//                Warnings = new List<DrugInteractionWarning>(),
//                HasInteractions = false
//            };
//        }
//    }

//    public async Task<List<DrugInteractionWarning>> GetKnownInteractionsAsync(List<string> medications)
//    {
//        // يمكن الاحتفاظ بهذه للدعم الخلفي، لكن من الأفضل استخدام CheckDrugInteractionsAsync مباشرة
//        throw new NotImplementedException("Use CheckDrugInteractionsAsync which uses Ingredients.");
//    }

//    public async Task<bool> AddDrugInteractionAsync(string medication1, string medication2, string severity, string description, string recommendation)
//    {
//        // هنا سنفترض caller يعطي أسماء المركبات (active ingredients)
//        try
//        {
//            var n1 = Normalize(medication1);
//            var n2 = Normalize(medication2);

//            // ensure ingredients exist or create them
//            var ing1 = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == n1)
//                       ?? new Ingredient { Name = medication1, NormalizedName = n1 };

//            var ing2 = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == n2)
//                       ?? new Ingredient { Name = medication2, NormalizedName = n2 };

//            if (ing1.Id == 0) _context.Ingredients.Add(ing1);
//            if (ing2.Id == 0) _context.Ingredients.Add(ing2);

//            await _context.SaveChangesAsync();

//            var a = Math.Min(ing1.Id, ing2.Id);
//            var b = Math.Max(ing1.Id, ing2.Id);

//            var exists = await _context.IngredientInteractions
//                .AnyAsync(ii => ii.IngredientAId == a && ii.IngredientBId == b);

//            if (exists) return false;

//            var interaction = new IngredientInteraction
//            {
//                IngredientAId = a,
//                IngredientBId = b,
//                Severity = severity,
//                Description = description,
//                Recommendation = recommendation
//            };

//            _context.IngredientInteractions.Add(interaction);
//            await _context.SaveChangesAsync();

//            return true;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error adding ingredient interaction {0} - {1}", medication1, medication2);
//            return false;
//        }
//    }

//    public async Task<List<string>> GetMedicationSuggestionsAsync(string partialName)
//    {
//        var normalized = Normalize(partialName);
//        var meds = await _context.Medications
//            .Where(m => m.NormalizedName.Contains(normalized))
//            .OrderBy(m => m.Name)
//            .Take(20)
//            .Select(m => m.Name)
//            .ToListAsync();

//        // if empty, fall back to common list or ingredients
//        if (!meds.Any())
//        {
//            meds = await _context.Ingredients
//                .Where(i => i.NormalizedName.Contains(normalized))
//                .Take(20)
//                .Select(i => i.Name)
//                .ToListAsync();
//        }

//        return meds;
//    }

//    private async Task<List<string>> GetPatientCurrentMedicationsAsync(int patientId)
//    {
//        // same as before: medication names from PrescriptionItems but we prefer to map to Medications table
//        try
//        {
//            var currentMedicationNames = await _context.PrescriptionItems
//                .Where(pi => pi.Prescription.PatientId == patientId &&
//                             pi.Prescription.Status == PrescriptionStatus.Dispensed &&
//                             pi.IsDispensed)
//                .Select(pi => pi.MedicationName)
//                .Distinct()
//                .ToListAsync();

//            return currentMedicationNames;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting current medications for patient {PatientId}", patientId);
//            return new List<string>();
//        }
//    }
//}





//********************************************************************************************
//public class DrugInteractionService : IDrugInteractionService
//{
//    private readonly MedicalRecordsDbContext _context;
//    private readonly ILogger<DrugInteractionService> _logger;
//    private readonly Dictionary<string, List<DrugInteraction>> _interactionDatabase;

//    public DrugInteractionService(
//        MedicalRecordsDbContext context,
//        ILogger<DrugInteractionService> logger)
//    {
//        _context = context;
//        _logger = logger;
//        _interactionDatabase = InitializeInteractionDatabase();
//    }

//    public async Task<DrugInteractionCheckResponse> CheckDrugInteractionsAsync(DrugInteractionCheckRequest request)
//    {
//        try
//        {
//            // Get patient's current medications
//            var currentMedications = await GetPatientCurrentMedicationsAsync(request.PatientId);

//            // Combine with new medications
//            var allMedications = currentMedications.Union(request.Medications).Distinct().ToList();

//            // Check for interactions
//            var warnings = await GetKnownInteractionsAsync(allMedications);

//            // Filter warnings to only include interactions with new medications
//            var relevantWarnings = warnings.Where(w => 
//                request.Medications.Contains(w.Medication1) || 
//                request.Medications.Contains(w.Medication2)).ToList();

//            return new DrugInteractionCheckResponse
//            {
//                Success = true,
//                Message = relevantWarnings.Any() ? "تم العثور على تفاعلات دوائية" : "لا توجد تفاعلات دوائية",
//                Warnings = relevantWarnings,
//                HasInteractions = relevantWarnings.Any()
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error checking drug interactions for patient {PatientId}", request.PatientId);
//            return new DrugInteractionCheckResponse
//            {
//                Success = false,
//                Message = "حدث خطأ أثناء فحص التفاعلات الدوائية",
//                Warnings = new List<DrugInteractionWarning>(),
//                HasInteractions = false
//            };
//        }
//    }

//    public async Task<List<DrugInteractionWarning>> GetKnownInteractionsAsync(List<string> medications)
//    {
//        var warnings = new List<DrugInteractionWarning>();

//        try
//        {
//            // Check all possible pairs of medications
//            for (int i = 0; i < medications.Count; i++)
//            {
//                for (int j = i + 1; j < medications.Count; j++)
//                {
//                    var medication1 = medications[i].ToLower();
//                    var medication2 = medications[j].ToLower();

//                    // Check for known interactions
//                    var interaction = FindInteraction(medication1, medication2);
//                    if (interaction != null)
//                    {
//                        warnings.Add(new DrugInteractionWarning
//                        {
//                            Medication1 = medications[i],
//                            Medication2 = medications[j],
//                            Severity = interaction.Severity,
//                            Description = interaction.Description,
//                            Recommendation = interaction.Recommendation
//                        });
//                    }
//                }
//            }

//            return warnings;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting known interactions for medications {Medications}", 
//                string.Join(", ", medications));
//            return warnings;
//        }
//    }

//    public async Task<bool> AddDrugInteractionAsync(string medication1, string medication2, string severity, string description, string recommendation)
//    {
//        try
//        {
//            // In a real implementation, you would save this to a database
//            // For now, we'll add it to the in-memory database
//            var key1 = $"{medication1.ToLower()}_{medication2.ToLower()}";
//            var key2 = $"{medication2.ToLower()}_{medication1.ToLower()}";

//            var interaction = new DrugInteraction
//            {
//                Medication1 = medication1.ToLower(),
//                Medication2 = medication2.ToLower(),
//                Severity = severity,
//                Description = description,
//                Recommendation = recommendation
//            };

//            _interactionDatabase[key1] = new List<DrugInteraction> { interaction };
//            _interactionDatabase[key2] = new List<DrugInteraction> { interaction };

//            return true;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error adding drug interaction between {Med1} and {Med2}", medication1, medication2);
//            return false;
//        }
//    }

//    public async Task<List<string>> GetMedicationSuggestionsAsync(string partialName)
//    {
//        try
//        {
//            // Common medications database
//            var commonMedications = new List<string>
//            {
//                "أسبرين", "باراسيتامول", "إيبوبروفين", "أموكسيسيلين", "سيفالكسين",
//                "ميتفورمين", "إنسولين", "أتورفاستاتين", "سيمفاستاتين", "أملوديبين",
//                "لوسارتان", "راميبريل", "فوروسيميد", "هيدروكلوروثيازيد", "وارفارين",
//                "كلوبيدوجريل", "أوميبرازول", "رانيتيدين", "دومبيريدون", "ميتوكلوبراميد",
//                "ديكلوفيناك", "كيتوبروفين", "نابروكسين", "ترامادول", "مورفين",
//                "ديكساميثازون", "بريدنيزولون", "هيدروكورتيزون", "سيتريزين", "لوراتادين",
//                "سودوإفيدرين", "فينيل إفرين", "سالبوتامول", "بوديزونيد", "تيوفيلين"
//            };

//            var suggestions = commonMedications
//                .Where(m => m.Contains(partialName, StringComparison.OrdinalIgnoreCase))
//                .Take(10)
//                .ToList();

//            return suggestions;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting medication suggestions for {PartialName}", partialName);
//            return new List<string>();
//        }
//    }

//    private async Task<List<string>> GetPatientCurrentMedicationsAsync(int patientId)
//    {
//        try
//        {
//            var currentMedications = await _context.PrescriptionItems
//                .Where(pi => pi.Prescription.PatientId == patientId && 
//                            pi.Prescription.Status == PrescriptionStatus.Dispensed &&
//                            pi.IsDispensed)
//                .Select(pi => pi.MedicationName)
//                .Distinct()
//                .ToListAsync();

//            return currentMedications;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting current medications for patient {PatientId}", patientId);
//            return new List<string>();
//        }
//    }

//    private DrugInteraction? FindInteraction(string medication1, string medication2)
//    {
//        var key1 = $"{medication1}_{medication2}";
//        var key2 = $"{medication2}_{medication1}";

//        if (_interactionDatabase.TryGetValue(key1, out var interactions1))
//        {
//            return interactions1.FirstOrDefault();
//        }

//        if (_interactionDatabase.TryGetValue(key2, out var interactions2))
//        {
//            return interactions2.FirstOrDefault();
//        }

//        return null;
//    }

//    private Dictionary<string, List<DrugInteraction>> InitializeInteractionDatabase()
//    {
//        return new Dictionary<string, List<DrugInteraction>>
//        {
//            // Warfarin interactions
//            ["وارفارين_أسبرين"] = new List<DrugInteraction>
//            {
//                new DrugInteraction
//                {
//                    Medication1 = "وارفارين",
//                    Medication2 = "أسبرين",
//                    Severity = "عالي",
//                    Description = "يزيد من خطر النزيف",
//                    Recommendation = "مراقبة INR بانتظام، تجنب الجرعات العالية من الأسبرين"
//                }
//            },
//            ["وارفارين_باراسيتامول"] = new List<DrugInteraction>
//            {
//                new DrugInteraction
//                {
//                    Medication1 = "وارفارين",
//                    Medication2 = "باراسيتامول",
//                    Severity = "متوسط",
//                    Description = "قد يزيد من تأثير الوارفارين",
//                    Recommendation = "مراقبة INR عند استخدام جرعات عالية من الباراسيتامول"
//                }
//            },

//            // Metformin interactions
//            ["ميتفورمين_وارفارين"] = new List<DrugInteraction>
//            {
//                new DrugInteraction
//                {
//                    Medication1 = "ميتفورمين",
//                    Medication2 = "وارفارين",
//                    Severity = "منخفض",
//                    Description = "قد يؤثر على مستويات السكر في الدم",
//                    Recommendation = "مراقبة مستويات السكر في الدم"
//                }
//            },

//            // ACE Inhibitor interactions
//            ["راميبريل_هيدروكلوروثيازيد"] = new List<DrugInteraction>
//            {
//                new DrugInteraction
//                {
//                    Medication1 = "راميبريل",
//                    Medication2 = "هيدروكلوروثيازيد",
//                    Severity = "متوسط",
//                    Description = "قد يسبب انخفاض ضغط الدم الشديد",
//                    Recommendation = "مراقبة ضغط الدم، البدء بجرعات منخفضة"
//                }
//            },

//            // NSAID interactions
//            ["إيبوبروفين_وارفارين"] = new List<DrugInteraction>
//            {
//                new DrugInteraction
//                {
//                    Medication1 = "إيبوبروفين",
//                    Medication2 = "وارفارين",
//                    Severity = "عالي",
//                    Description = "يزيد من خطر النزيف المعوي",
//                    Recommendation = "تجنب الاستخدام المشترك، استخدام الباراسيتامول كبديل"
//                }
//            },

//            // Antibiotic interactions
//            ["أموكسيسيلين_وارفارين"] = new List<DrugInteraction>
//            {
//                new DrugInteraction
//                {
//                    Medication1 = "أموكسيسيلين",
//                    Medication2 = "وارفارين",
//                    Severity = "متوسط",
//                    Description = "قد يزيد من تأثير الوارفارين",
//                    Recommendation = "مراقبة INR أثناء العلاج"
//                }
//            }
//        };
//    }
//}

// Helper class for drug interactions
//public class DrugInteraction
//{
//    public string Medication1 { get; set; } = string.Empty;
//    public string Medication2 { get; set; } = string.Empty;
//    public string Severity { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//    public string Recommendation { get; set; } = string.Empty;
//}

//**************************************




//public async Task<DrugInteractionCheckResponse> CheckDrugInteractionsAsync(DrugInteractionCheckRequest request)
//{
//    try
//    {
//        // 1) patient current medications (names) -> get their ingredients
//        var currentMedications = await GetPatientCurrentMedicationsAsync(request.PatientId);

//        // 2) combine medication names (original strings)
//        var allMedicationNames = currentMedications
//             .Union(request.Medications)
//             .Distinct(StringComparer.OrdinalIgnoreCase)
//             .ToList();

//        // 3) map medication names -> medication records (if exist)
//        var normalizedNames = allMedicationNames.Select(n => Normalize(n)).ToList();

//        var medications = await _context.Medications
//            .Where(m => normalizedNames.Contains(m.NormalizedName))
//            .Include(m => m.MedicationIngredients)
//                .ThenInclude(mi => mi.Ingredient)
//            .ToListAsync();

//        // 4) for any medication name not mapped, try fuzzy match by startingWith or suggest to admin
//        var mappedNames = medications.Select(m => m.NormalizedName).ToHashSet();
//        var unknownMedicationNames = allMedicationNames
//            .Where(n => !mappedNames.Contains(Normalize(n)))
//            .ToList();

//        // (Optional) you may log or return suggestions for unknown meds so admin can map them

//        // 5) collect ingredient IDs
//        var ingredientIds = medications
//            .SelectMany(m => m.MedicationIngredients.Select(mi => mi.IngredientId))
//            .Distinct()
//            .ToList();

//        // 6) If there are unknown meds, you could try to lookup by ingredient name directly if request includes them (not in current DTO)

//        // 7) fetch interactions for any pair of ingredients
//        var warnings = new List<DrugInteractionWarning>();
//        if (ingredientIds.Count >= 2)
//        {
//            // get all interactions where A in ids and B in ids
//            // ensuring we check both orders: we stored A<B invariantly
//            var interactions = await _context.IngredientInteractions
//                .Where(ii => ingredientIds.Contains(ii.IngredientAId) && ingredientIds.Contains(ii.IngredientBId))
//                .Include(ii => ii.IngredientA)
//                .Include(ii => ii.IngredientB)
//                .ToListAsync();

//            // Map interactions to medication names: for user clarity, we map ingredient -> medication name(s) involved
//            foreach (var inter in interactions)
//            {
//                // find medication names that contain these ingredients
//                var medsA = medications
//                    .Where(m => m.MedicationIngredients.Any(mi => mi.IngredientId == inter.IngredientAId))
//                    .Select(m => m.Name)
//                    .Distinct()
//                    .ToList();

//                var medsB = medications
//                    .Where(m => m.MedicationIngredients.Any(mi => mi.IngredientId == inter.IngredientBId))
//                    .Select(m => m.Name)
//                    .Distinct()
//                    .ToList();

//                // for clarity, report one representative med name from each side (or list all)
//                var medNameA = medsA.FirstOrDefault() ?? inter.IngredientA.Name;
//                var medNameB = medsB.FirstOrDefault() ?? inter.IngredientB.Name;

//                // Only show warnings that involve at least one medication from the incoming request (new medications)
//                bool involvesNewMed = request.Medications
//                    .Select(n => Normalize(n))
//                    .Intersect(medications.Select(m => m.NormalizedName))
//                    .Any();

//                warnings.Add(new DrugInteractionWarning
//                {
//                    Medication1 = medNameA,
//                    Medication2 = medNameB,
//                    Severity = inter.Severity,
//                    Description = inter.Description,
//                    Recommendation = inter.Recommendation
//                });
//            }
//        }

//        var relevantWarnings = warnings
//            // optionally filter to only include those where at least one of the meds is in request.Medications
//            .Where(w =>
//                request.Medications.Any(r => Normalize(r) == Normalize(w.Medication1)) ||
//                request.Medications.Any(r => Normalize(r) == Normalize(w.Medication2)))
//            .ToList();

//        return new DrugInteractionCheckResponse
//        {
//            Success = true,
//            Message = relevantWarnings.Any() ? "تم العثور على تفاعلات دوائية" : "لا توجد تفاعلات دوائية",
//            Warnings = relevantWarnings,
//            HasInteractions = relevantWarnings.Any()
//        };
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error checking drug interactions for patient {PatientId}", request.PatientId);
//        return new DrugInteractionCheckResponse
//        {
//            Success = false,
//            Message = "حدث خطأ أثناء فحص التفاعلات الدوائية",
//            Warnings = new List<DrugInteractionWarning>(),
//            HasInteractions = false
//        };
//    }
//}
//***************************************************
//public async Task<ServiceResult> CreatePrescriptionAsync(PrescriptionCreateRequest request)
//{
//    // تحقق من وجود المريض والطبيب
//    var patient = await _context.Patients.FindAsync(request.PatientId);
//    var doctor = await _context.Doctors.FindAsync(request.DoctorId);

//    if (patient == null) return ServiceResult.Fail("المريض غير موجود");
//    if (doctor == null) return ServiceResult.Fail("الطبيب غير موجود");

//    // إنشاء الفاتورة
//    var prescription = new Prescription
//    {
//        PatientId = request.PatientId,
//        DoctorId = request.DoctorId,
//        Diagnosis = request.Diagnosis,                
//        Status = PrescriptionStatus.Pending,
//        PrescriptionDate = DateTime.UtcNow,
//        CreatedAt = DateTime.UtcNow
//    };

//    _context.Prescriptions.Add(prescription);
//    await _context.SaveChangesAsync();

//    // إضافة العناصر (الأدوية)
//    foreach (var item in request.Items)
//    {
//        var drug = await _context.Drugs.FindAsync(item.DrugId);
//        if (drug == null) return ServiceResult.Fail($"الدواء بالمعرف {item.DrugId} غير موجود");

//        var prescriptionItem = new PrescriptionItem
//        {
//            PrescriptionId = prescription.Id,
//            DrugId = drug.Id,
//            Dosage = item.Dosage,
//            Frequency = item.Frequency,
//            Duration = item.Duration,
//            Instructions = item.Instructions,
//            Quantity = item.Quantity,
//            CreatedAt = DateTime.UtcNow
//        };

//        _context.PrescriptionItems.Add(prescriptionItem);
//    }

//    await _context.SaveChangesAsync();

//    return ServiceResult.Ok("تم إضافة الوصفة الطبية بنجاح");
//}

//************************************************
// استرجاع التفاعلات المعروفة لكل زوج من المواد الفعّالة


// اقتراح أسماء الأدوية عند كتابة الوصفة
//public async Task<List<DrugSuggestionDto>> GetDrugSuggestionsAsync(string partialName)
//{
//    try
//    {
//        return await _context.Drugs
//            .Where(d => d.ScientificName.Contains(partialName) ||
//                        d.BrandName.Contains(partialName) ||
//                        d.ChemicalName.Contains(partialName))
//            .Select(d => new DrugSuggestionDto
//            {
//                DrugId = d.Id,
//                ScientificName = d.ScientificName,
//                BrandName = d.BrandName,
//                ChemicalName = d.ChemicalName
//            })
//            .Take(10)
//            .ToListAsync();
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error getting drug suggestions for {PartialName}", partialName);
//        return new List<DrugSuggestionDto>();
//    }
//}

//*****************************************


//public async Task<List<string>> GetMedicationSuggestionsAsync(string partialName)
//{
//    var normalized = Normalize(partialName);
//    var meds = await _context.Medications
//        .Where(m => m.NormalizedName.Contains(normalized))
//        .OrderBy(m => m.Name)
//        .Take(20)
//        .Select(m => m.Name)
//        .ToListAsync();

//    // if empty, fall back to common list or ingredients
//    if (!meds.Any())
//    {
//        meds = await _context.Ingredients
//            .Where(i => i.NormalizedName.Contains(normalized))
//            .Take(20)
//            .Select(i => i.Name)
//            .ToListAsync();
//    }

//    return meds;
//}

//private async Task<List<string>> GetPatientCurrentMedicationsAsync(int patientId)
//{
//    // same as before: medication names from PrescriptionItems but we prefer to map to Medications table
//    try
//    {
//        var currentMedicationNames = await _context.PrescriptionItems
//            .Where(pi => pi.Prescription.PatientId == patientId &&
//                         pi.Prescription.Status == PrescriptionStatus.Dispensed &&
//                         pi.IsDispensed)
//            .Select(pi => pi.MedicationName)
//            .Distinct()
//            .ToListAsync();

//        return currentMedicationNames;
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error getting current medications for patient {PatientId}", patientId);
//        return new List<string>();
//    }
//}
//public async Task<bool> AddDrugInteractionAsync(string medication1, string medication2, string severity, string description, string recommendation)
//{
//    // هنا سنفترض caller يعطي أسماء المركبات (active ingredients)
//    try
//    {
//        var n1 = Normalize(medication1);
//        var n2 = Normalize(medication2);

//        // ensure ingredients exist or create them
//        var ing1 = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == n1)
//                   ?? new Ingredient { Name = medication1, NormalizedName = n1 };

//        var ing2 = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == n2)
//                   ?? new Ingredient { Name = medication2, NormalizedName = n2 };

//        if (ing1.Id == 0) _context.Ingredients.Add(ing1);
//        if (ing2.Id == 0) _context.Ingredients.Add(ing2);

//        await _context.SaveChangesAsync();

//        var a = Math.Min(ing1.Id, ing2.Id);
//        var b = Math.Max(ing1.Id, ing2.Id);

//        var exists = await _context.IngredientInteractions
//            .AnyAsync(ii => ii.IngredientAId == a && ii.IngredientBId == b);

//        if (exists) return false;

//        var interaction = new IngredientInteraction
//        {
//            IngredientAId = a,
//            IngredientBId = b,
//            Severity = severity,
//            Description = description,
//            Recommendation = recommendation
//        };

//        _context.IngredientInteractions.Add(interaction);
//        await _context.SaveChangesAsync();

//        return true;
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error adding ingredient interaction {0} - {1}", medication1, medication2);
//        return false;
//    }
//}


//*****************************************


