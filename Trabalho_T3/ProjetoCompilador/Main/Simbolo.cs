namespace Main
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


        public Simbolo(string lexema, string token, object tipo)
        {
            this.Lexema = lexema;
            this.Token = token;
            this.Tipo = tipo;
        }

        public Simbolo()
        {
        }
               
        public Simbolo CopiaAtributos()
        {
            Simbolo novo = new Simbolo
            {
                Lexema = this.Lexema,
                Coluna = this.Coluna,
                Linha = this.Linha,
                Tipo = this.Tipo,
                Token = this.Token,
                DescricaoERRO = this.DescricaoERRO

            };

            return novo;
        }
    }

  
}
