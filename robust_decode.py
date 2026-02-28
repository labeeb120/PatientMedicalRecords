import base64
import io
from PIL import Image
from pyzbar.pyzbar import decode
import sys

def decode_qr_from_b64_file(filename):
    try:
        with open(filename, 'r') as f:
            b64_data = f.read().strip()

        # Remove data URI prefix if present
        if b64_data.startswith('data:image'):
            b64_data = b64_data.split(',')[1]

        # Clean the string (remove potential weird characters at the end)
        # Base64 chars are A-Z, a-z, 0-9, +, / and =
        cleaned_b64 = ""
        for char in b64_data:
            if char.isalnum() or char in "+/=":
                cleaned_b64 += char
            else:
                # Stop at first non-base64 char if it's near the end
                break

        # Pad if needed
        missing_padding = len(cleaned_b64) % 4
        if missing_padding:
            cleaned_b64 += '=' * (4 - missing_padding)

        img_bytes = base64.b64decode(cleaned_b64)
        img = Image.open(io.BytesIO(img_bytes))

        results = decode(img)
        if not results:
            print("No QR code detected.")
            return

        for obj in results:
            print(f"Decoded Data: {obj.data.decode('utf-8')}")

    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    decode_qr_from_b64_file('user_provided_b64.txt')
