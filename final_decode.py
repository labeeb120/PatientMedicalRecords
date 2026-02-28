import base64
from pyzbar.pyzbar import decode
from PIL import Image
import io

with open('user_b64.txt', 'r') as f:
    b64_data = f.read().strip()

# Base64 strings starting with data:image/png;base64, need the prefix removed
if ',' in b64_data:
    b64_data = b64_data.split(',')[1]

try:
    # Add padding if necessary
    missing_padding = len(b64_data) % 4
    if missing_padding:
        b64_data += '=' * (4 - missing_padding)

    img_data = base64.b64decode(b64_data)
    img = Image.open(io.BytesIO(img_data))
    img.save('decoded_qr.png')

    decoded_objects = decode(img)
    if decoded_objects:
        print("Decoded Content:")
        print(decoded_objects[0].data.decode('utf-8'))
    else:
        print("No QR code found in the image.")
except Exception as e:
    print(f"Error: {e}")
