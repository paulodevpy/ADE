using System.Security.Cryptography;
using System.Text;

namespace ADE.Core.Audit;

/// <summary>
/// Fonte única de verdade para o cálculo do hash encadeado dos eventos
/// de auditoria. Centralizar a fórmula evita divergência entre o módulo
/// que grava (AuditChain/AuditService) e o que verifica
/// (AuditChainVerifier) — divergência que invalidaria a cadeia.
/// </summary>
public static class AuditHashHelper
{
    /// <summary>
    /// Monta a representação canônica do evento usada como base do hash.
    /// Inclui o horário confiável (NTP) para que qualquer adulteração do
    /// carimbo de tempo quebre a cadeia.
    /// </summary>
    public static string BuildCanonicalString(
        AuditEvent evt)
    {
        return
            $"{evt.EventNumber}" +
            $"{evt.TimestampUtc:o}" +
            $"{evt.TrustedTimeUtc?.ToString("o") ?? ""}" +
            $"{evt.TimeSource}" +
            $"{evt.Action}" +
            $"{evt.Data}" +
            $"{evt.PreviousHash}";
    }

    /// <summary>
    /// Calcula o hash SHA-256 (hex) do evento a partir da forma canônica.
    /// </summary>
    public static string ComputeHash(
        AuditEvent evt)
    {
        string raw =
            BuildCanonicalString(evt);

        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(raw)));
    }
}
