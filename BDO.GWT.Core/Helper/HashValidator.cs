using BDO.GWT.Domain.Core.Services;
using BDO.GWT.Xperience.Core.Services;
using System;
using System.Security.Cryptography;
using System.Text;

namespace BDO.GWT.Domain.Core.Helpers;

public interface IHashValidator
{
    bool ValidateEmailHash(string email, string hash);
}

public class HashValidator : IHashValidator
{
    private readonly string _hashSalt;

    public HashValidator(IHashSaltProvider hashSaltProvider)
    {
        _hashSalt = hashSaltProvider.HashSalt;
    }

    public bool ValidateEmailHash(string email, string hash)
    {
        return GetSHA256Hash($"{email}{_hashSalt}") == hash;
    }

    private static string GetSHA256Hash(string inputData)
    {
        using var algorithm = SHA256.Create();
        return GetHash(inputData, algorithm);
    }

    private static string GetHash(string inputData, HashAlgorithm algorithm)
    {
        var bytes = Encoding.UTF8.GetBytes(inputData);
        return Convert.ToHexString(algorithm.ComputeHash(bytes)).ToLower();
    }
}
