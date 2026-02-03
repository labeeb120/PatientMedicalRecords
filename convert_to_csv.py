import json
import csv

with open('yemen_drugs_import.json', 'r', encoding='utf-8') as f:
    drugs = json.load(f)

with open('yemen_drugs_import.csv', 'w', encoding='utf-8', newline='') as f:
    writer = csv.DictWriter(f, fieldnames=["ScientificName", "BrandName", "ChemicalName", "Manufacturer", "Ingredients"])
    writer.writeheader()
    for d in drugs:
        # Convert list to comma-separated string
        d_copy = d.copy()
        d_copy['Ingredients'] = ", ".join(d_copy['Ingredients'])
        writer.writerow(d_copy)

with open('yemen_interactions_import.json', 'r', encoding='utf-8') as f:
    interactions = json.load(f)

with open('yemen_interactions_import.csv', 'w', encoding='utf-8', newline='') as f:
    writer = csv.DictWriter(f, fieldnames=["IngredientAName", "IngredientBName", "Severity", "Description", "Recommendation"])
    writer.writeheader()
    for i in interactions:
        writer.writerow(i)

print("Converted to CSV")
