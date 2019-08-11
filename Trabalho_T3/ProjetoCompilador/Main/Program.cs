using System;
using System.Collections.Generic;
using System.IO; //biblioteca para ler e escrever dados em arquivos.

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            var codigoFonte = File.ReadAllText(@"CodigoFonte2.txt");  // Transforma o arquivo todo em string
            codigoFonte = codigoFonte.Replace(' ', '\r');

            var analisadorSintatico = new AnalisadorSintatico(codigoFonte);

            analisadorSintatico.AnaliseSintatica();
        }
    }
}

