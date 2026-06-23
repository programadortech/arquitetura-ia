# Integração: Storage de Arquivos (`IFileStorage`)

Porta: `IFileStorage` (Application). Seleção: `Integrations:Storage:Provider`. Segredos via env/secret store.

## Provedores suportados
| Provedor | Quando usar | Prós | Contras | Secret |
|---|---|---|---|---|
| **Local** | Dev / single-node | Simples, sem custo | Não escala/HA | — |
| **Amazon S3** | AWS, padrão de mercado | Durável, barato, presigned URLs | Setup IAM | credenciais AWS |
| **Azure Blob** | Stack Azure | Integra Azure, tiers | — | `AZURE_STORAGE_CONNECTION` |
| **Google Cloud Storage** | Stack GCP | Integra GCP | — | service account |
| **MinIO** | On-premise S3-compatível | Self-hosted, API S3 | Operar você mesmo | chaves MinIO |

## Recomendação
- **Dev:** Local. **Nuvem:** o storage da nuvem do produto (S3/Blob/GCS). **On-premise:** MinIO (API S3).

## Contrato
```csharp
public interface IFileStorage
{
    Task<Result<string>> SaveAsync(string path, Stream content, string contentType, CancellationToken ct);
    Task<Result<Stream>> GetAsync(string path, CancellationToken ct);
    Task<Result> DeleteAsync(string path, CancellationToken ct);
}
```
Adicione com `/create-integration storage <provider>`.
