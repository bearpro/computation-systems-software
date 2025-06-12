import os
import sys
import argparse
from pdf2image import convert_from_path
import pytesseract
from PIL import Image

def convert_pdf_to_images(pdf_path, dpi=300, poppler_path=None):
    """
    Convert each page of the PDF to a PIL Image.
    Requires poppler on Windows: set poppler_path to the bin folder.
    """
    return convert_from_path(pdf_path, dpi=dpi, poppler_path=poppler_path)


def ocr_images(images, lang='eng', config='--psm 3'):
    """
    Perform OCR on a list of PIL Images.
    Returns the concatenated text.
    """
    texts = []
    for i, img in enumerate(images, start=1):
        text = pytesseract.image_to_string(img, lang=lang, config=config)
        texts.append(f"\n\n===== Page {i} =====\n\n")
        texts.append(text)
    return ''.join(texts)


def write_text(output_path, text):
    """
    Write OCR result to a text file.
    """
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(text)


def main():
    parser = argparse.ArgumentParser(description='OCR a rasterized PDF into text.')
    parser.add_argument('pdf', help='Path to input PDF file')
    parser.add_argument('-o', '--output', help='Path to output text file', default=None)
    parser.add_argument('--dpi', type=int, help='Resolution for PDF rendering (default: 300)', default=300)
    parser.add_argument('--lang', help='Tesseract language code (default: eng)', default='eng')
    parser.add_argument('--poppler-path', help='Path to Poppler bin directory (Windows only)')
    parser.add_argument('--tesseract-cmd', help='Full path to tesseract.exe', default=None)
    args = parser.parse_args()

    # Configure Tesseract command if provided
    if args.tesseract_cmd:
        pytesseract.pytesseract.tesseract_cmd = args.tesseract_cmd
    
    if not os.path.isfile(args.pdf):
        print(f"Error: PDF file '{args.pdf}' not found.", file=sys.stderr)
        sys.exit(1)

    output_path = args.output or os.path.splitext(args.pdf)[0] + '_ocr.txt'

    print("Converting PDF pages to images...")
    images = convert_pdf_to_images(args.pdf, dpi=args.dpi, poppler_path=args.poppler_path)
    print(f"Converted {len(images)} pages.")

    print("Performing OCR on images...")
    text = ocr_images(images, lang=args.lang)

    print(f"Writing OCR output to '{output_path}'...")
    write_text(output_path, text)

    print("Done.")

if __name__ == '__main__':
    main()

# Usage (Windows):
# 1. Install dependencies:
#    pip install pdf2image pillow pytesseract
# 2. Install Poppler for Windows: https://github.com/oschwartz10612/poppler-windows/releases
#    Unzip and note the bin folder path.
# 3. Install Tesseract-OCR: https://github.com/tesseract-ocr/tesseract
#    Default path: C:\\Program Files\\Tesseract-OCR\\tesseract.exe
# 4. Run:
#    python ocr_pdf.py input.pdf -o output.txt --poppler-path "C:\\path\\to\\poppler\\bin" --tesseract-cmd "C:\\Program Files\\Tesseract-OCR\\tesseract.exe"
