from Cryptodome.Cipher import AES
from cryptography.hazmat.primitives import padding
from Crypto.Util.Padding import pad
import hashlib

def encrypt(filename, password, salt):
    # ファイルの内容をバイト配列として読み込む
    with open(filename, "rb") as f:
        input_bytes = f.read()
    
    salt = salt.encode('utf-8')
    encrypted_data = encrypt_data(input_bytes, password, salt)

    # 暗号化されたデータを別のファイルに書き込む（拡張子は.encにする）
    new_file_name = filename + ".enc"
    with open(new_file_name, "wb") as f:
        f.write(encrypted_data)

def encrypt_data(input_bytes, password, salt):
    iterations = 100    
    
    key_derive = hashlib.pbkdf2_hmac('sha256', password.encode(), salt, iterations=iterations, dklen=48) # キー導出関数
    key = key_derive[:32] # キー
    iv = key_derive[32:] # 初期化ベクトル
    

    aes = AES.new(key, AES.MODE_CBC, iv)

    encrypted_data = aes.encrypt(pad(input_bytes, 16))

    return encrypted_data
        

def decrypt(filename, password, salt):
    iterations = 100
    
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