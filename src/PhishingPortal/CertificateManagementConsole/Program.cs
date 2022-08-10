using CertificateManager;
using CertificateManager.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static CreateCertificates? _cc;
    static void Main(string[] args)
    {
        CreateCertification(args);
    }


    static void CreateCertification(string[] args)
    {
        var cn = "PhishingPortal.UI.Blazor";
        var password = "1234";
        var outputPath = string.Empty;

        if (args.Length == 1)
            cn = args[0];

        if (args.Length == 2)
            password = args[1];

        if (args.Length == 3)
            outputPath = args[2];

        var sp = new ServiceCollection()
           .AddCertificateManager()
           .BuildServiceProvider();

        _cc = sp.GetService<CreateCertificates>();

        if (_cc == null)
        {
            Console.WriteLine("CreateCertificate object not found");
            return;
        }

        var rsaCert = CreateRsaCertificate(cn, 10);
        var ecdsaCert = CreateECDsaCertificate(cn, 10);

        var iec = sp.GetService<ImportExportCertificate>();

        var rsaCertPfxBytes = iec.ExportSelfSignedCertificatePfx(password, rsaCert);
        File.WriteAllBytes(Path.Combine(outputPath, "cert_rsa512.pfx"), rsaCertPfxBytes);

        var ecdsaCertPfxBytes = iec.ExportSelfSignedCertificatePfx(password, ecdsaCert);
        File.WriteAllBytes(Path.Combine(outputPath, "cert_ecdsa384.pfx"), ecdsaCertPfxBytes);

        Console.WriteLine("Certificates created created");
    }

    public static X509Certificate2 CreateRsaCertificate(string dnsName, int validityPeriodInYears)
    {
        var basicConstraints = new BasicConstraints
        {
            CertificateAuthority = false,
            HasPathLengthConstraint = false,
            PathLengthConstraint = 0,
            Critical = false
        };

        var subjectAlternativeName = new SubjectAlternativeName
        {
            DnsName = new List<string>
                {
                    dnsName,
                }
        };

        var x509KeyUsageFlags = X509KeyUsageFlags.DigitalSignature;

        // only if certification authentication is used
        var enhancedKeyUsages = new OidCollection
            {
                OidLookup.ClientAuthentication,
                OidLookup.ServerAuthentication 
                // OidLookup.CodeSigning,
                // OidLookup.SecureEmail,
                // OidLookup.TimeStamping  
            };

        var certificate = _cc.NewRsaSelfSignedCertificate(
            new DistinguishedName { CommonName = dnsName },
            basicConstraints,
            new ValidityPeriod
            {
                ValidFrom = DateTimeOffset.UtcNow,
                ValidTo = DateTimeOffset.UtcNow.AddYears(validityPeriodInYears)
            },
            subjectAlternativeName,
            enhancedKeyUsages,
            x509KeyUsageFlags,
            new RsaConfiguration
            {
                KeySize = 2048,
                HashAlgorithmName = HashAlgorithmName.SHA512
            });

        return certificate;
    }

    public static X509Certificate2 CreateECDsaCertificate(string dnsName, int validityPeriodInYears)
    {
        var basicConstraints = new BasicConstraints
        {
            CertificateAuthority = false,
            HasPathLengthConstraint = false,
            PathLengthConstraint = 0,
            Critical = false
        };

        var subjectAlternativeName = new SubjectAlternativeName
        {
            DnsName = new List<string>
                {
                    dnsName,
                }
        };

        var x509KeyUsageFlags = X509KeyUsageFlags.DigitalSignature;

        // only if certification authentication is used
        var enhancedKeyUsages = new OidCollection {
                OidLookup.ClientAuthentication,
                OidLookup.ServerAuthentication 
                // OidLookup.CodeSigning,
                // OidLookup.SecureEmail,
                // OidLookup.TimeStamping 
            };

        var certificate = _cc.NewECDsaSelfSignedCertificate(
            new DistinguishedName { CommonName = dnsName },
            basicConstraints,
            new ValidityPeriod
            {
                ValidFrom = DateTimeOffset.UtcNow,
                ValidTo = DateTimeOffset.UtcNow.AddYears(validityPeriodInYears)
            },
            subjectAlternativeName,
            enhancedKeyUsages,
            x509KeyUsageFlags,
            new ECDsaConfiguration
            {
                KeySize = 384,
                HashAlgorithmName = HashAlgorithmName.SHA384
            });

        return certificate;
    }
}