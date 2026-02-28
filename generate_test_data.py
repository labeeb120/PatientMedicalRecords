import json
import random
from datetime import datetime, timedelta

def generate_arabic_name():
    first_names = ["أحمد", "محمد", "علي", "فاطمة", "سارة", "خالد", "ليلى", "عمر", "هند", "يوسف", "مريم", "عبدالله", "نورة", "سعد", "منى", "إبراهيم", "جواهر", "سلطان", "عبير", "فيصل"]
    last_names = ["المنصوري", "القحطاني", "الزهراني", "الشمري", "العتيبي", "المالكي", "الحربي", "الغامدي", "الدوسري", "الرويلي", "المطيري", "العنزي", "السبيعي", "الشهري", "البقمي", "التميمي", "السهلي", "الرشيدي", "الفضلي", "الخالدي"]
    return f"{random.choice(first_names)} {random.choice(last_names)}"

def generate_test_data(count=20):
    blood_types = [1, 2, 3, 4, 5, 6, 7, 8] # Enum values
    genders = [1, 2] # 1: Male, 2: Female
    allergies_list = ["بنسلين", "لاكتوز", "مكسرات", "غبار", "فراولة", "سمك", "بيض", "فول صويا"]
    diseases_list = ["السكري", "الضغط", "الربو", "الغدة الدرقية", "القولون العصبي", "الروماتيزم"]
    surgeries_list = ["المرارة", "الزائدة الدودية", "الفتق", "تجميل الأنف", "القسطرة"]
    meds_list = ["أدول", "بروفين", "أوميبرازول", "ميتفورمين", "كونكور", "أوجمنتين", "سولبادين"]

    all_data = []

    for i in range(1, count + 1):
        patient_id = i
        full_name = generate_arabic_name()

        # 1. Initialize Profile Data
        init_profile = {
            "fullName": full_name,
            "dateOfBirth": (datetime.now() - timedelta(days=random.randint(6000, 20000))).strftime("%Y-%m-%dT%H:%M:%S"),
            "gender": random.choice(genders),
            "phoneNumber": f"050{random.randint(1000000, 9999999)}",
            "email": f"patient{i}@example.com",
            "address": f"شارع {random.randint(1, 100)}، حي {random.choice(['الأمانة', 'النزهة', 'الروضة', 'الياسمين'])}",
            "bloodType": random.choice(blood_types),
            "weight": random.randint(50, 110),
            "height": random.randint(150, 195),
            "emergencyContact": f"قريب {i}",
            "emergencyPhone": f"055{random.randint(1000000, 9999999)}",
            "allergies": [{"allergenName": random.choice(allergies_list), "reaction": "طفح جلدي", "severity": "متوسط"}],
            "chronicDiseases": [{"diseaseName": random.choice(diseases_list), "description": "تحت السيطرة", "diagnosisDate": "2020-01-01"}],
            "surgeries": [{"surgeryName": random.choice(surgeries_list), "description": "عملية ناجحة", "surgeryDate": "2019-05-10", "hospital": "مستشفى الأمل", "surgeon": "د. فهد"}],
            "currentMedications": [{"medicationName": random.choice(meds_list), "dosage": "500mg", "frequency": "مرتين يومياً", "duration": "مستمر", "instructions": "بعد الأكل"}],
            "notes": "المريض لديه تاريخ عائلي مع أمراض القلب"
        }

        # 2. Medical Record Data
        medical_record = {
            "patientId": patient_id,
            "diagnosis": f"اشتباه {random.choice(['التهاب الجيوب الأنفية', 'انفلونزا موسمية', 'نقص فيتامين د', 'إجهاد عضلي'])}",
            "notes": "تم إجراء فحص سريري وكافة العلامات الحيوية طبيعية",
            "symptoms": "صداع مستمر وخمول",
            "treatment": "راحة تامة مع الالتزام بالوصفة المرفقة",
            "recordDate": datetime.now().strftime("%Y-%m-%dT%H:%M:%S")
        }

        # 3. Prescription Data
        prescription = {
            "patientId": patient_id,
            "diagnosis": medical_record["diagnosis"],
            "notes": "الالتزام بالجرعات في أوقاتها المحددة",
            "items": [
                {
                    "medicationName": random.choice(meds_list),
                    "dosage": "100mg",
                    "frequency": "مرة واحدة قبل النوم",
                    "duration": "7 أيام",
                    "instructions": "لا يشرب معه حليب",
                    "quantity": 1
                },
                {
                    "medicationName": "فيتامين سي",
                    "dosage": "1000mg",
                    "frequency": "فوار مرة يومياً",
                    "duration": "10 أيام",
                    "instructions": "بعد الغداء",
                    "quantity": 2
                }
            ]
        }

        all_data.append({
            "patientName": full_name,
            "patientId": patient_id,
            "initProfileRequest": init_profile,
            "medicalRecordRequest": medical_record,
            "prescriptionRequest": prescription
        })

    return all_data

test_data = generate_test_data(20)
print(json.dumps(test_data, ensure_ascii=False, indent=2))
