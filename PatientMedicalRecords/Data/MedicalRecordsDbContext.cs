using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Models;
using BCrypt;
namespace PatientMedicalRecords.Data
{
    public class MedicalRecordsDbContext : DbContext
    {
        public MedicalRecordsDbContext(DbContextOptions<MedicalRecordsDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<PrescriptionDispense> PrescriptionDispenses { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<ChronicDisease> ChronicDiseases { get; set; }
        public DbSet<Surgery> Surgeries { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        // existing DBSets...
        // public DbSet<Medication> Medications { get; set; } // Deprecated 26-01-2026
        public DbSet<Ingredient> Ingredients { get; set; }
        // public DbSet<MedicationIngredient> MedicationIngredients { get; set; } // Deprecated 26-01-2026
        // public DbSet<IngredientInteraction> IngredientInteractions { get; set; } // Deprecated 26-01-2026

        public DbSet<Drug> Drugs { get; set; }
        public DbSet<DrugIngredient> DrugIngredients { get; set; }
        public DbSet<DrugInteraction> DrugInteractions { get; set; }
        public DbSet<UserAttachment> UserAttachments { get; set; }
        
        //25-01-2026
        public DbSet<PinnedPatient> PinnedPatients { get; set; }

        //26-01-2026  „ «÷«›… Â–« «·”ÿ— ·œ⁄„ ⁄·«ﬁ… «·⁄œÌœ ≈·Ï «·⁄œÌœ »Ì‰ «·„” Œœ„ Ê«·’·«ÕÌ« 
        public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            // **********************************************
            //  ﬂÊÌ‰ ⁄·«ﬁ«  DrugInteraction <-> Ingredient
            // **********************************************

            // 1.  ﬂÊÌ‰ «·⁄·«ﬁ… „‰ IngredientA ≈·Ï DrugInteraction
            modelBuilder.Entity<DrugInteraction>()
                .HasOne(di => di.IngredientA)// DrugInteraction ·œÌÂ IngredientA
                .WithMany(i => i.DrugInteractionAsA)// Ingredient ·œÌÂ ﬁ«∆„… DrugInteractionAsA
                .HasForeignKey(di => di.IngredientAId)// «·„› «Õ «·Œ«—ÃÌ ÂÊ IngredientAId
                .OnDelete(DeleteBehavior.Restrict); // ‰Ê’Ì »„‰⁄ «·Õ–› «·„  «·Ì ·„‰⁄ ›ﬁœ«‰ «· ›«⁄·« // „Â„: Ì„‰⁄ «· ⁄«—÷

            // 2.  ﬂÊÌ‰ «·⁄·«ﬁ… „‰ IngredientB ≈·Ï DrugInteraction
            modelBuilder.Entity<DrugInteraction>()
                .HasOne(di => di.IngredientB)// DrugInteraction ·œÌÂ IngredientB
                .WithMany(i => i.DrugInteractionAsB)// Ingredient ·œÌÂ ﬁ«∆„… DrugInteractionAsB
                .HasForeignKey(di => di.IngredientBId)// «·„› «Õ «·Œ«—ÃÌ ÂÊ IngredientBId
                .OnDelete(DeleteBehavior.Restrict); // ‰Ê’Ì »„‰⁄ «·Õ–› «·„  «·Ì// „Â„: Ì„‰⁄ «· ⁄«—÷

            // ≈÷«›… ›Â—” ›—Ìœ ·÷„«‰ «· ›—œ
            
            // ≈÷«›… ›Â—” ›—Ìœ ·÷„«‰ ⁄œ„  ﬂ—«— «· ›«⁄·«  (ÊÌ›—÷ «· — Ì» A < B)
            // ÌÃ» «· √ﬂœ „‰ √‰ «· — Ì» Ì „ »‘ﬂ· ’ÕÌÕ ›Ì Ã«‰» «·ﬂÊœ (Seeding Data)
            modelBuilder.Entity<DrugInteraction>()
                .HasIndex(di => new { di.IngredientAId, di.IngredientBId })
                .IsUnique();

            //*******************************************
            //*****************************************
            // **********************************************
            // 1. ≈œŒ«· «·„—ﬂ»«  «·‰‘ÿ… (Ingredients)
            // **********************************************

            var paracetamol = new Ingredient { Id = 1, Name = "Paracetamol", NormalizedName = "paracetamol" };
            var ibuprofen = new Ingredient { Id = 2, Name = "Ibuprofen", NormalizedName = "ibuprofen" };
            var amoxicillin = new Ingredient { Id = 3, Name = "Amoxicillin", NormalizedName = "amoxicillin" };
            var warfarin = new Ingredient { Id = 4, Name = "Warfarin", NormalizedName = "warfarin" }; // „„Ì⁄ œ„

            modelBuilder.Entity<Ingredient>().HasData(
                paracetamol,
                ibuprofen,
                amoxicillin,
                warfarin
            );

            // **********************************************
            // 2. ≈œŒ«· «·√œÊÌ… (Drugs)
            // **********************************************

            var drug1 = new Drug { Id = 1, ScientificName = "Paracetamol", BrandName = "Adol", NormalizedName = "adol", Manufacturer = "Pharma Co." };
            var drug2 = new Drug { Id = 2, ScientificName = "Ibuprofen", BrandName = "Brufen", NormalizedName = "brufen", Manufacturer = "Med Co." };
            var drug3 = new Drug { Id = 3, ScientificName = "Amoxicillin", BrandName = "Amoxil", NormalizedName = "amoxil", Manufacturer = "Global Drugs" };
            var drug4 = new Drug { Id = 4, ScientificName = "Paracetamol", BrandName = "Tylenol", NormalizedName = "tylenol", Manufacturer = "US Pharma" };
            var drug5 = new Drug { Id = 5, ScientificName = "Warfarin", BrandName = "Coumadin", NormalizedName = "coumadin", Manufacturer = "Chem Lab" };

            modelBuilder.Entity<Drug>().HasData(drug1, drug2, drug3, drug4, drug5);

            // **********************************************
            // 3. —»ÿ «·√œÊÌ… »«·„—ﬂ»«  «·‰‘ÿ… (DrugIngredients)
            // **********************************************

            modelBuilder.Entity<DrugIngredient>().HasData(
                // Adol (1) -> Paracetamol (1)
                new DrugIngredient { //Id = 1,
                    DrugId = 1, IngredientId = 1 },
                // Brufen (2) -> Ibuprofen (2)
                new DrugIngredient {// Id = 2,
                    DrugId = 2, IngredientId = 2 },
                // Amoxil (3) -> Amoxicillin (3)
                new DrugIngredient { DrugId = 3, IngredientId = 3 },
                // Tylenol (4) -> Paracetamol (1)
                new DrugIngredient { DrugId = 4, IngredientId = 1 },
                // Coumadin (5) -> Warfarin (4)
                new DrugIngredient { DrugId = 5, IngredientId = 4 }
            );

            // **********************************************
            // 4. ≈œŒ«· «· ›«⁄·«  «·œÊ«∆Ì… (DrugInteractions)
            // **********************************************

            // «· ›«⁄· 1: Ibuprofen (2) Ê Warfarin (4) - Œÿ— ﬂ»Ì—
            // ‰” Œœ„ «·√’€— (2) ›Ì A Ê«·√ﬂ»— (4) ›Ì B ·÷„«‰  — Ì» ›—Ìœ
            modelBuilder.Entity<DrugInteraction>().HasData(
                new DrugInteraction
                {
                    Id = 1,
                    IngredientAId = 2, // Ibuprofen
                    IngredientBId = 4, // Warfarin
                    Severity = "Major",
                    Description = "ﬁœ Ì“Ìœ «·≈Ì»Ê»—Ê›Ì‰ „‰  √ÀÌ— «·Ê«—›«—Ì‰° „„« Ì“Ìœ »‘ﬂ· ﬂ»Ì— „‰ Œÿ— «·‰“Ì› «·Õ«œ.",
                    Recommendation = " Ã‰» «·«” Œœ«„ «·„‘ —ﬂ. ÌÃ» «” Œœ«„ „”ﬂ‰ »œÌ· („À· »«—«”Ì «„Ê·) Ê„—«ﬁ»… INR »«‰ Ÿ«„."
                },
                // «· ›«⁄· 2: Paracetamol (1) Ê Amoxicillin (3) - ·« ÌÊÃœ  ›«⁄· ”—Ì—Ì ﬂ»Ì— (Minor)
                new DrugInteraction
                {
                    Id = 2,
                    IngredientAId = 1, // Paracetamol
                    IngredientBId = 3, // Amoxicillin
                    Severity = "None/Minor",
                    Description = "·« ÌÊÃœ  ›«⁄· ”—Ì—Ì ﬂ»Ì— „⁄—Ê› »Ì‰ Â–Ì‰ «·„ﬂÊ‰Ì‰.",
                    Recommendation = "Ì„ﬂ‰ «·«” Œœ«„ «·„‘ —ﬂ »√„«‰."
                }
            );




            //*******************************************


            //  ÿ»Ìﬁ ﬁ«⁄œ… ⁄œ„  ﬂ—«— ⁄·«ﬁ… Drug-Ingredient
            modelBuilder.Entity<DrugIngredient>()
                .HasIndex(di => new { di.DrugId, di.IngredientId })
                .IsUnique();

            // **********************************************
            // 2.  ﬂÊÌ‰ ⁄·«ﬁ… Drug <-> DrugIngredient <-> Ingredient
            // **********************************************

            ////  ﬂÊÌ‰ «·⁄·«ﬁ… «·„—ﬂ»… ··—»ÿ Many-to-Many
            //modelBuilder.Entity<DrugIngredient>()
            //    .HasKey(di => di.Id); // «” Œœ«„ «·„› «Õ «·√”«”Ì «·⁄«œÌ «·–Ì Ê÷⁄ Â

            modelBuilder.Entity<DrugIngredient>()
                .HasOne(di => di.Drug)
                .WithMany(d => d.DrugIngredients)
                .HasForeignKey(di => di.DrugId);

            modelBuilder.Entity<DrugIngredient>()
                .HasOne(di => di.Ingredient)
                .WithMany(i => i.DrugIngredients)
                .HasForeignKey(di => di.IngredientId);

            //*****************************************

            // modelBuilder.Entity<MedicationIngredient>()
            // .HasKey(mi => new { mi.MedicationId, mi.IngredientId });

            // modelBuilder.Entity<MedicationIngredient>()
            //     .HasOne(mi => mi.Medication)
            //     .WithMany(m => m.MedicationIngredients)
            //     .HasForeignKey(mi => mi.MedicationId);

           



            //*****************************************************
            modelBuilder.Entity<DrugIngredient>()
        .HasKey(di => new { di.DrugId, di.IngredientId });

            modelBuilder.Entity<DrugIngredient>()
                .HasOne(di => di.Drug)
                .WithMany(d => d.DrugIngredients)
                .HasForeignKey(di => di.DrugId);

            modelBuilder.Entity<DrugIngredient>()
                .HasOne(di => di.Ingredient)
                .WithMany(i => i.DrugIngredients)
                .HasForeignKey(di => di.IngredientId);

            //modelBuilder.Entity<IngredientInteraction>()
            //    .HasOne(ii => ii.PrimaryIngredient)
            //    .WithMany(i => i.InteractionsAsPrimary)
            //    .HasForeignKey(ii => ii.PrimaryIngredientId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<IngredientInteraction>()
            //    .HasOne(ii => ii.InteractsWithIngredient)
            //    .WithMany(i => i.InteractionsAsInteractsWith)
            //    .HasForeignKey(ii => ii.InteractsWithIngredientId)
            //    .OnDelete(DeleteBehavior.Restrict);


            //*****************************************************

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.NationalId).IsUnique();
                entity.Property(e => e.NationalId).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                //entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });

            // Patient Configuration
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithOne(p => p.Patient)
                    .HasForeignKey<Patient>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

                  modelBuilder.Entity<Patient>()
                    .HasIndex(p => p.PatientCode)
                    .IsUnique();


            // Doctor Configuration
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithOne(p => p.Doctor)
                    .HasForeignKey<Doctor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Pharmacist Configuration
            modelBuilder.Entity<Pharmacist>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithOne(p => p.Pharmacist)
                    .HasForeignKey<Pharmacist>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MedicalRecord Configuration
            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.MedicalRecords)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Doctor)
                    .WithMany(p => p.MedicalRecords)
                    .HasForeignKey(d => d.DoctorId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Prescription Configuration
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.Prescriptions)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Doctor)
                    .WithMany(p => p.Prescriptions)
                    .HasForeignKey(d => d.DoctorId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PrescriptionItem Configuration
            modelBuilder.Entity<PrescriptionItem>(entity =>
            {
                entity.HasOne(d => d.Prescription)
                    .WithMany(p => p.PrescriptionItems)
                    .HasForeignKey(d => d.PrescriptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PrescriptionDispense Configuration
            modelBuilder.Entity<PrescriptionDispense>(entity =>
            {
                entity.HasOne(d => d.Prescription)
                    .WithMany(p => p.PrescriptionDispenses)
                    .HasForeignKey(d => d.PrescriptionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Pharmacist)
                    .WithMany(p => p.PrescriptionDispenses)
                    .HasForeignKey(d => d.PharmacistId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Allergy Configuration
            modelBuilder.Entity<Allergy>(entity =>
            {
                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.Allergies)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ChronicDisease Configuration
            modelBuilder.Entity<ChronicDisease>(entity =>
            {
                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.ChronicDiseases)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Surgery Configuration
            modelBuilder.Entity<Surgery>(entity =>
            {
                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.Surgeries)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AccessToken Configuration
            modelBuilder.Entity<AccessToken>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.AccessTokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token).IsUnique();
            });

            // AuditLog Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // --- «·ŒÿÊ… «·√Ê·Ï: “—⁄ »Ì«‰«  «·„” Œœ„ ›Ì ÃœÊ· "Users" ---
            // Â–« «·Ã“¡ Ì»ﬁÏ ﬂ„« ÂÊ  ﬁ—Ì»«° Ê·ﬂ‰ „⁄ Õ–› Œ«’Ì… "Role"
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 101,
                    NationalId = "1000000001",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRole.Admin, // <-- Â–« «·”ÿ—  „ Õ–›Â ·√‰Â ·„ Ì⁄œ „ÊÃÊœ«
                    Status = UserStatus.Approved,
                    FullName = "System Administrator",
                    Email = "admin@medicalrecords.com",
                    CreatedAt = DateTime.UtcNow
                }
            
            );

            modelBuilder.Entity<UserRoleAssignment>().HasData(
                new UserRoleAssignment
                {
                    Id = 1,
                    UserId = 101,
                    Role = UserRole.Admin
                }
            );
        }


















        //private void SeedData(ModelBuilder modelBuilder)
        //{
        //    // Create default admin user
        //    modelBuilder.Entity<User>().HasData(
        //        new User
        //        {

        //            Id = 101,
        //            NationalId = "1000000001",
        //            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        //            Role = UserRole.Admin,
        //            Status = UserStatus.Approved,
        //            FullName = "System Administrator",
        //            Email = "admin@medicalrecords.com",
        //            CreatedAt = DateTime.UtcNow
        //        }

        //    );
        //}

    }
}
