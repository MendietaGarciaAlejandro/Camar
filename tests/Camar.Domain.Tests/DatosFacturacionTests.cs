using Camar.Domain.Common;
using Camar.Domain.Members;

namespace Camar.Domain.Tests;

/// <summary>
/// Estos algoritmos ya estan escritos en la libreria validadores-es, en Kotlin. Aqui se
/// repiten en C# a proposito: el servidor no puede fiarse de que el cliente haya validado,
/// porque cualquiera puede llamar a la API sin pasar por la aplicacion.
/// </summary>
public class TaxIdTests
{
    [Theory]
    [InlineData("12345678Z")]
    [InlineData("00000000T")]
    public void Nif_ConLetraCorrecta_SeAcepta(string texto)
    {
        var documento = new TaxId(texto);

        Assert.Equal(TaxIdKind.Nif, documento.Kind);
        Assert.Equal(texto, documento.Value);
    }

    [Theory]
    [InlineData("X1234567L")]
    [InlineData("Y0000000Z")]
    public void Nie_ConLetraCorrecta_SeAcepta(string texto)
    {
        Assert.Equal(TaxIdKind.Nie, new TaxId(texto).Kind);
    }

    [Theory]
    [InlineData("A28015865")]  // Telefonica
    [InlineData("A15075062")]  // Inditex
    [InlineData("Q2826004J")]  // Universidad Complutense, con control alfabetico
    public void Cif_DeEmpresasReales_SeAcepta(string texto)
    {
        Assert.Equal(TaxIdKind.Cif, new TaxId(texto).Kind);
    }

    [Fact]
    public void Normaliza_EspaciosGuionesYMinusculas()
    {
        Assert.Equal("12345678Z", new TaxId(" 12345678-z ").Value);
    }

    [Theory]
    [InlineData("12345678A")]   // la letra no corresponde
    [InlineData("A28015866")]   // el control del CIF no cuadra
    [InlineData("W1234567L")]   // inicial que no es X, Y ni Z
    [InlineData("1234567Z")]    // le falta una cifra
    public void DocumentoInvalido_SeRechaza(string texto)
    {
        Assert.Throws<BusinessRuleException>(() => new TaxId(texto));
    }

    [Fact]
    public void LaTablaDeLetrasCubreLosVeintitresRestos()
    {
        const string letras = "TRWAGMYFPDXBNJZSQVHLCKE";

        for (var resto = 0; resto < 23; resto++)
        {
            var numero = 23_000_000 + resto;
            var nif = $"{numero:D8}{letras[resto]}";

            Assert.Equal(TaxIdKind.Nif, new TaxId(nif).Kind);
        }
    }
}

public class BankAccountTests
{
    [Theory]
    [InlineData("ES9121000418450200051332")]
    [InlineData("ES7921000813610123456789")]
    [InlineData("DE89370400440532013000")]
    public void IbanValido_SeAcepta(string texto)
    {
        Assert.Equal(texto, new BankAccount(texto).Value);
    }

    [Fact]
    public void Normaliza_LaSeparacionEnGruposDeCuatro()
    {
        var cuenta = new BankAccount("ES91 2100 0418 4502 0005 1332");

        Assert.Equal("ES9121000418450200051332", cuenta.Value);
        Assert.Equal("ES91 2100 0418 4502 0005 1332", cuenta.Formatted());
        Assert.Equal("ES", cuenta.Country);
    }

    [Theory]
    [InlineData("ES9121000418450200051333")]  // una cifra cambiada
    [InlineData("ES9021000418450200051332")]  // digitos de control cambiados
    [InlineData("ES9121000418450200053132")]  // dos cifras intercambiadas
    public void IbanInvalido_SeRechaza(string texto)
    {
        Assert.Throws<BusinessRuleException>(() => new BankAccount(texto));
    }
}

public class ContactoTests
{
    [Theory]
    [InlineData("600112233")]
    [InlineData("+34600112233")]
    [InlineData("0034 600 11 22 33")]
    public void Telefono_ConYSinPrefijo_SeNormalizaIgual(string texto)
    {
        Assert.Equal("600112233", new PhoneNumber(texto).Value);
    }

    [Fact]
    public void Telefono_DistingueMovilDeFijo()
    {
        Assert.True(new PhoneNumber("600112233").IsMobile);
        Assert.False(new PhoneNumber("911223344").IsMobile);
    }

    [Theory]
    [InlineData("100112233")]  // primera cifra que no existe
    [InlineData("60011223")]   // le falta una cifra
    public void TelefonoInvalido_SeRechaza(string texto)
    {
        Assert.Throws<BusinessRuleException>(() => new PhoneNumber(texto));
    }

    [Theory]
    [InlineData("28001", 28)]
    [InlineData("08001", 8)]
    [InlineData("52001", 52)]
    public void CodigoPostal_DeduceLaProvincia(string texto, int provincia)
    {
        Assert.Equal(provincia, new PostalCode(texto).ProvinceCode);
    }

    [Theory]
    [InlineData("00001")]  // la provincia 00 no existe
    [InlineData("53001")]  // ni la 53
    [InlineData("2800")]   // le falta una cifra
    public void CodigoPostalInvalido_SeRechaza(string texto)
    {
        Assert.Throws<BusinessRuleException>(() => new PostalCode(texto));
    }
}
