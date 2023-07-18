from command import exe
from encrypt import encrypt, decrypt
from qr_code import create
from yaml_reader import write, read

def add_file(filename):
    password = "fogfogfog"
    salt = "adfweffwaefwa"
    encrypt(filename, password, salt)
    # 暗号化されたファイルをIPFSに登録する
    out = exe(f"ipfs add {filename}.enc")   
    cid = out.split(" ")[1]
    print(f"{filename} -> {cid}")
    return cid

def create_qr_code(filename):    
    cid = add_file(filename)

    obj = {
        "name": filename,
        "cid": cid,
    }

    meta_file_name="test.yaml"
    qr_code_name="qr.png"

    write(meta_file_name, obj)
    cid = add_file(meta_file_name)

    create(qr_code_name, cid)
    print(f"{cid} -> {qr_code_name}")
    
    
convert_file_name="test.ssh"
# convert_file_name=r"E:\Projects\research\DecentralizedMetaverse\build\DCMetaverse\DCMetaverse_Data\StreamingAssets\content\VRChat_2023-02-02_15-14-41.644_1920x1080.png"
create_qr_code(convert_file_name)
