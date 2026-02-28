from pyzbar.pyzbar import decode
from PIL import Image
import sys

try:
    data = decode(Image.open('test_qr.png'))
    if data:
        print(data[0].data.decode('utf-8'))
    else:
        print("No QR code found")
except Exception as e:
    print(f"Error: {e}")
