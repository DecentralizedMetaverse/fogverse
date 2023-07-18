import os
from Crypto.Cipher import AES
from Crypto.Protocol.KDF import PBKDF2


def encrypt(input_bytes, password, salt):    
    # ソルトを生成する
    salt = os.urandom(8)

    # キーとIVを導出する
    key_derive = PBKDF2(password, salt)
    key = key_derive[:32]
    iv = key_derive[32:48]

    # AES暗号化オブジェクトを作る
    aes = AES.new(key, AES.MODE_CBC, iv)

    # データをパディングする
    pad_len = 16 - len(input_bytes) % 16
    input_bytes += bytes([pad_len] * pad_len)

    # 暗号化されたデータを格納するバイト配列を作る
    encrypted_data = salt + aes.encrypt(input_bytes)
    
    return encrypted_data
    
def decrypt(encrypted_data, password, salt):
    # ソルトとデータを分離する
    salt = encrypted_data[:8]
    data = encrypted_data[8:]

    # キーとIVを導出する
    key_derive = PBKDF2(password, salt)
    key = key_derive[:32]
    iv = key_derive[32:48]

    # AES復号化オブジェクトを作る
    aes = AES.new(key, AES.MODE_CBC, iv)

    # データを復号化する
    decrypted_data = aes.decrypt(data)

    # パディングを除去する
    pad_len = decrypted_data[-1]
    decrypted_data = decrypted_data[:-pad_len]

    return decrypted_data

password = "test"
salt = "test1"

# 暗号化
f = open("test.ssh", "rb")
txt = f.read()
f.close()

encrypted_data = encrypt(txt, password, salt)
f = open("test.ssh", "rb")
txt = f.read()
f.close()


print()

# 復号化