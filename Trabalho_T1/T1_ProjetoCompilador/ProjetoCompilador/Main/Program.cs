using System;
using System.Collections.Generic;
using System.IO; //biblioteca para ler e escrever dados em arquivos.
using AnalisadorLexico;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            var codigoFonte = File.ReadAllText(@"CodigoFonte.txt");  // Transforma o arquivo todo em string
            codigoFonte = codigoFonte.Replace(' ', '\r');

            AnalisadorLexico.AnalisadorLexico analisadorLexico = new AnalisadorLexico.AnalisadorLexico(codigoFonte);

            Simbolo simbolo = new Simbolo();
            Console.WriteLine("Aperte ENTER para solicitar o TOKEN:");

            while (simbolo.Lexema != "EOF")
            {
                string caracterDigitado = Console.ReadLine();
                if (caracterDigitado == "\n");
                simbolo = analisadorLexico.RetornaToken();
                EscreveSimboloNaTela(simbolo);
            }

            Console.ReadLine();

            List<Simbolo> TabelaDeSimbolos = analisadorLexico.GetTabelaDeSimbolos();
        }

        private static void EscreveSimboloNaTela(Simbolo s)
        {
            if(s.Token == "ERRO")
            {
                Console.WriteLine($@"Token: {s.Token} Descrição: {s.DescricaoERRO} Linha: {s.LinhaDoERRO} Coluna: {s.ColunaDoERRO}");
            }
            else
            {
                Console.WriteLine($@"Lexema: { s.Lexema} | Token: { s.Token} | Tipo : { (s.Tipo == null ? "null" : s.Tipo)} ");
            }
            
        }
    }
}

