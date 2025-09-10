# import secrets
# import base64

# Generate a 256-bit (32 bytes) secret
# secret = secrets.token_bytes(32)

# secret = secrets.token_hex(32)
# print("JWT Secret:", secret)

# Encode as base64 for easy use in config files or environment variables
# base64_secret = base64.urlsafe_b64encode(secret).decode('utf-8')
# print("JWT Secret (base64):", base64_secret)


import uuid

uuid_64 = str(uuid.uuid4()).replace('-', '') + str(uuid.uuid4()).replace('-', '')

print("64-char concatenated UUID4s:", uuid_64)
