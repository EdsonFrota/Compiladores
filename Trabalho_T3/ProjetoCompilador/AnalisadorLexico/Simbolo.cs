namespace AnalisadorLexico
{
    public class Simbolo // é o que o analisador léxico esta gerando
    {

        // Um simbolo pode ter esses atributos
        public string Lexema { get; set; } 

        public string Token { get; set; }

        public object Tipo { get; set; }

        public string DescricaoERRO { get; set; }

        public int Linha{ get; set; }

        public int Coluna { get; set; }

       
    }
}
