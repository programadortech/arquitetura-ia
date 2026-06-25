namespace Plataforma2ASmart.Auth.IntegrationTests;

public class PlaceholderTests
{
    [Fact]
    public void Projeto_de_integracao_compila_e_roda()
        => Assert.True(true);

    [Fact(Skip = "Requer infraestrutura real (SQL Server via Testcontainers). Implementar com /create-tests.")]
    public void Roundtrip_repositorio_pendente()
    {
    }
}
