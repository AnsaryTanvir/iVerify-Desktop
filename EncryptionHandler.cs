using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

class EncryptionHandler
{
    private RSACryptoServiceProvider rsaPublic;
    private RSACryptoServiceProvider rsaPrivate;

    public void LoadKeys(string publicKeyPath, string privateKeyPath)
    {
        // Load public key
        string publicKeyText = File.ReadAllText(publicKeyPath);
        rsaPublic = PemToRsaProvider(publicKeyText, false);

        // Load private key
        string privateKeyText = File.ReadAllText(privateKeyPath);
        rsaPrivate = PemToRsaProvider(privateKeyText, true);
    }

    private RSACryptoServiceProvider PemToRsaProvider(string pem, bool isPrivateKey)
    {
        PemReader pemReader = new PemReader(new StringReader(pem));
        object pemObject = pemReader.ReadObject();

        if (isPrivateKey)
        {
            if (pemObject is AsymmetricCipherKeyPair keyPair)
            {
                RsaPrivateCrtKeyParameters privateKeyParams = (RsaPrivateCrtKeyParameters)keyPair.Private;
                RSAParameters rsaParams = DotNetUtilities.ToRSAParameters(privateKeyParams);
                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
                rsaProvider.ImportParameters(rsaParams);
                return rsaProvider;
            }
        }
        else
        {
            if (pemObject is RsaKeyParameters publicKeyParams)
            {
                RSAParameters rsaParams = DotNetUtilities.ToRSAParameters(publicKeyParams);
                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
                rsaProvider.ImportParameters(rsaParams);
                return rsaProvider;
            }
        }

        throw new InvalidOperationException("Invalid PEM file content.");
    }

    public string ExportPublicKey()
    {
        return rsaPublic.ToXmlString(false); // false to get the public key
    }

    public string ExportPrivateKey()
    {
        return rsaPrivate.ToXmlString(true); // true to get the private key
    }

    public byte[] EncryptData(string data)
    {
        byte[] dataToEncrypt = Encoding.UTF8.GetBytes(data);
        return rsaPublic.Encrypt(dataToEncrypt, true); // true to use OAEP padding
    }

    public string DecryptData(byte[] encryptedData)
    {
        byte[] decryptedData = rsaPrivate.Decrypt(encryptedData, true); // true to use OAEP padding
        return Encoding.UTF8.GetString(decryptedData);
    }

    public void DisplayKeys()
    {
        MessageBox.Show("Public Key:");
        MessageBox.Show(ExportPublicKey());

        MessageBox.Show("Private Key:");
        MessageBox.Show(ExportPrivateKey());
    }

    public void CloseKeys()
    {
        rsaPublic.Dispose();
        rsaPrivate.Dispose();
    }

    ~EncryptionHandler()
    {
        CloseKeys();
    }
}