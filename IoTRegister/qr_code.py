import qrcode

def create(file_name, txt):
    img = qrcode.make(txt)
    img.save(file_name)