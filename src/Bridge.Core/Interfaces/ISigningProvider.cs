using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Models;

namespace Bridge.Core.Interfaces;

public interface ISigningProvider
{
    ProviderInfo GetProviderInfo();

    Task<IReadOnlyList<Certificate>> GetCertificatesAsync(CancellationToken cancellationToken = default);

    Task<bool> CanSignAsync(string certificateId, CancellationToken cancellationToken = default);

    Task<byte[]> SignAsync(
        string certificateId,
        string hashAlgorithm,
        byte[] data,
        CancellationToken cancellationToken = default
    );

    Task<System.Security.Cryptography.X509Certificates.X509Certificate2?> GetX509CertificateAsync(
        string certificateId,
        CancellationToken cancellationToken = default
    );
}
