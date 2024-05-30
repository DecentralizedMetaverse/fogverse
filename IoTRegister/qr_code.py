import qrcode

def create(file_name, txt):
    img = qrcode.make(txt)
    img.save(file_name)
    
    
if __name__ == '__main__':
    import sys 
    create(f"{sys.argv[2]}.png", sys.argv[1])