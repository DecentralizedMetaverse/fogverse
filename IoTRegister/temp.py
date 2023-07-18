from Cryptodome.Cipher import AES
from Cryptodome.Random import get_random_bytes
from Cryptodome.Protocol.KDF import PBKDF2
from cryptography.hazmat.primitives import padding
from Crypto.Util.Padding import pad
import hashlib

def encrypt(filename, password, salt):
    iterations = 10000
    # ファイルを読み込む
    with open(filename, "rb") as f:
        data = f.read()
        
    password = password.encode('utf-8')
    salt = salt.encode('utf-8')
    
    # PBKDF2で鍵と初期化ベクトルを生成（32バイトと16バイト）
    key = hashlib.pbkdf2_hmac("sha256", password, salt, iterations, 32)
    iv = hashlib.pbkdf2_hmac("sha256", password, salt, iterations, 16)

    # AES暗号器を作成
    # cipher = AES.new(key, AES.MODE_CBC, iv)
    cipher = AES.new(key, AES.MODE_CBC, iv)

    # データの長さが16バイトの倍数になるようにパディングする
    # padder = padding.PKCS7(128).padder()
    # data = padder.update(data) + padder.finalize()
    
    # ファイルを暗号化
    # encrypted_data = cipher.encrypt(data)
    encrypted_data = cipher.encrypt(pad(data, AES.block_size))

    # 暗号化したファイルを書き込む
    with open(f"{filename}.enc", "wb") as f:
        f.write(encrypted_data) # saltを先頭に書き込む
        

def decrypt(filename, password, salt):
    iterations = 10000
    
    # ファイルを読み込む
    with open(filename, "rb") as f:
        encrypted_data = f.read()
        
    # パディングを取り除く
    unpadder = padding.PKCS7(128).unpadder()
    data = unpadder.update(data) + unpadder.finalize()
        
    password = password.encode('utf-8')
    salt = salt.encode('utf-8')
        
    # PBKDF2で鍵と初期化ベクトルを生成（32バイトと16バイト）
    key = hashlib.pbkdf2_hmac("sha256", password, salt, iterations, 32)
    iv = hashlib.pbkdf2_hmac("sha256", password, salt, iterations, 16)

    # パスワードとsaltから32バイトのキーを生成（PBKDF2関数を使用）
    # key = PBKDF2(password, salt, dkLen=32)

    # AES暗号器を作成
    decipher = AES.new(key, AES.MODE_CBC, iv)

    # ファイルを復号化
    decrypted_data = decipher.decrypt(encrypted_data)
    
    decrypted_data = decrypted_data.rstrip(b"\x00")

    # 復号化したファイルを書き込む
    idx = filename.rfind(".")
    filename = filename[0:idx]
    with open(filename, "wb") as f:
        f.write(decrypted_data)