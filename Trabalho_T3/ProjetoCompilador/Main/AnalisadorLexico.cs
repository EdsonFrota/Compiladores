using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace Main
{
    public class AnalisadorLexico
    {
        public AnalisadorLexico(string codigoFonte)          // Instância o analisador léxico tendo como parametro o código fonte.
        {
            this._codigoFonte = codigoFonte;             // Acessa o codigo fonte 
        }

        #region Criação de Atributos
        private int _coluna;                        // variavel para a ler coluna da tabela de simbolos
        int _linha;                               // variavel para ler as linhas da tabela de símbolo
        char _caracterAtual;
        int _tipo_de_erro = -1;
        int _ponteiro;
        string _proxEstado = "q0";
        string _estadoAnterior = "q0";
        string _lexema = "";                        // variável que vai formar o lexema 
        int _linhaDoEstado = 0;
        int _colunaDaEntrada = 0;
        int _linhaAnterior = 0;
        string _codigoFonte;
        int _contador_de_Erros = 0;
        int _contadorDeSimbolos = 0;
        List<Simbolo> _listaDeErros = new List<Simbolo>();
        static Dictionary<string, Simbolo> PalavrasChave = Palavras_Chave();
        Dictionary<string, int> SimbolosValidos = Simbolos_Validos();
        Dictionary<string, int> Abertura = Simbolos_Especiais();
        static Dictionary<string, int> fechamento = Fechamento();
        Dictionary<int, string> TiposErros = Tipos_de_Erros();
        Dictionary<string, int> EstadosFinais = Estados_Finais();
        Dictionary<string, int> TodosEstados = Estados().Union(Estados_Finais()).ToDictionary(k => k.Key, v => v.Value);
        static List<Simbolo> TabelaDeSimbolos = PalavrasChave.Values.ToList();
        static DataTable dt = CarregaTabelaDeTransicao();
        static EnumerableRowCollection<DataRow> tableEnumerable = dt.AsEnumerable();
        static DataRow[] TabelaDeTransicao = tableEnumerable.ToArray();
        #endregion

        public Simbolo AnaliseLexica(string codigoFonte)  // Percorre ocodigo fonte do Mgol
        {
            _proxEstado = "q0"; // a cada inicialização ele deve começar do estado incial
            _estadoAnterior = "q0";
            _lexema = "";   // lexema vazio
            _linhaDoEstado = 0;
            _colunaDaEntrada = 0;
            _tipo_de_erro = -1;

            while (_ponteiro != codigoFonte.Length) // percorre o ponteiro enquanto ele não chegar no final do codigo fonte
            {
                //_ponteiro -> do codigo fonte
                _caracterAtual = codigoFonte[_ponteiro];

                ModificaLinhaColuna(); // Contador que atualiza o valor da linha e da coluna

                if (!SimbolosValidos.ContainsKey(_caracterAtual.ToString())) //verifica se o caractere atual é valido
                {// se não for caractere válido retorna o erro
                    _tipo_de_erro = 0;
                    _proxEstado = "ERRO";
                }

                _estadoAnterior = _proxEstado;  //armazena o estado anterior para o proximo estado ser modificado

                TodosEstados.TryGetValue(_proxEstado, out _linhaDoEstado);

                SimbolosValidos.TryGetValue(_caracterAtual.ToString(), out _colunaDaEntrada); //Modifica o valor da coluna de acordo com o caractere

                if (_estadoAnterior == "num" && (_caracterAtual == 'E' || _caracterAtual == 'e'))
                    _colunaDaEntrada = 9;

                _proxEstado = Convert.ToString(TabelaDeTransicao[_linhaDoEstado][_colunaDaEntrada]);

                if (EstadosFinais.ContainsKey(_estadoAnterior) && _proxEstado == "")
                {
                    if (_estadoAnterior == "qG")
                        _estadoAnterior = "opr";

                    //VERIFICA SE TEM ESSE LEXEMA NA LISTA DE SIMBOLOS
                    Simbolo s = TabelaDeSimbolos.Find(o => o.Lexema == _lexema);

                    if (s == null) // s-> simbolo do codigo fonte
                    {
                        s = new Simbolo() { Lexema = _lexema, Token = _estadoAnterior, Tipo = null };

                        if (_estadoAnterior.Equals("id"))
                            TabelaDeSimbolos.Add(s); // lista de identificadores

                        s = PreencheAtributos(s, _tipo_de_erro); //Verifica se ocorreu algum erro de caracter inválido durante a leitura

                        return s;
                    }
                    else
                    {
                        s = PreencheAtributos(s, _tipo_de_erro);//Verifica se ocorreu algum erro de caracter inválido durante a leitura

                        return s;
                    }
                }

                if (!char.IsWhiteSpace(_caracterAtual) || _proxEstado == "qA") // Verifica se o caractere atual é espaço em  branco ou se o proximo estado é qA
                {
                    if (_caracterAtual == '\r') //Converte o \r para ' '
                        _caracterAtual = ' ';

                    _lexema += _caracterAtual;
                }

                _ponteiro++;

                continue; // Continua o laço de análise
            }

            Simbolo simb = TabelaDeSimbolos.Find(o => o.Lexema == _lexema); // verifica se o lexema formado está na tabela de simbolo

            if (simb == null)   // verifica se chegou no final da leitura do arquivo
            {
                simb = new Simbolo { Token = "$" };
                _linha++;
                PreencheAtributos(simb, _tipo_de_erro);
            }

            //if (_contador_de_Erros != 0)
            //{
            //    foreach (Simbolo s in _listaDeErros)
            //        Console.WriteLine($@"Token: {s.Token} Descrição: {s.DescricaoERRO} Linha: {s.Linha} Coluna: {s.Coluna}");
            //}

            return simb;
        }

        private Simbolo CriaErro(int codErro)
        {
            //Informações necessárias para a criação de um erro
            Simbolo s = new Simbolo();

            string _descricaoERRO = "";
            s.Linha = ++_linha;
            s.Coluna = _coluna;
            TiposErros.TryGetValue(codErro, out _descricaoERRO);
            s.DescricaoERRO = _descricaoERRO;

            return (s);
        }


        private Simbolo PreencheAtributos(Simbolo s, int cod_erro)
        {
            if (cod_erro >= 0) //Caso ocorra um erro, as seguintes informações são solicitadas
            {
                string _descricaoERRO = "";
                TiposErros.TryGetValue(cod_erro, out _descricaoERRO);
                s.DescricaoERRO = _descricaoERRO;
                s.Coluna = _coluna;
                s.Linha = _linha;

                return s;
            }

            switch (s.Token)
            {
                case "num":
                    if (s.Lexema.Contains("."))
                        s.Tipo = "double";
                    else
                        s.Tipo = "int";
                    break;
                case "lit":
                    s.Tipo = "literal";
                    break;
                case "opm":
                    s.Tipo = s.Lexema;
                    break;
                case "rcb":
                    s.Tipo = "=";
                    break;
                case "opr":
                    s.Tipo = s.Lexema;
                    break;
            }

            s.Coluna = _coluna;
            s.Linha = _linha;

            return s;
        }

        private void ModificaLinhaColuna()  // altera o valor da coluna e da linha de acordo com o caractere
        {
            _linhaAnterior = _linha;

            if (_estadoAnterior == "q0" && _caracterAtual == '\n')
                _linha++;

            if (_caracterAtual == '\t')
            {
                _coluna += 4;
                return;
            }

            if (_linhaAnterior != _linha)
                _coluna = 0;
            else
                _coluna++;
        }

        public Simbolo RetornaToken()
        {
            return AnaliseLexica(_codigoFonte);
        }

        public List<Simbolo> GetTabelaDeSimbolos()
        {
            return TabelaDeSimbolos;
        }

        public static DataTable CarregaTabelaDeTransicao() //método para ler a tabela de transição, arquivo em excel
        {
            string arquivoExcel = @"TabelaDeTransicao.xlsx";

            DataTable Data_Table = new DataTable();

            //Pega o endereço do arquivo excel para abrir
            string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source =" + arquivoExcel + "; Extended Properties = 'Excel 8.0;HDR=YES'";
            OleDbConnection conn = new OleDbConnection(connectionString);
            OleDbCommand cmd = new OleDbCommand();
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter();
            cmd.Connection = conn;
            conn.Open();

            DataTable dtSchema;
            dtSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            string nomePlanilha = dtSchema.Rows[0]["TABLE_NAME"].ToString();
            conn.Close();
            //le todos os dados da planilha para o Data Table    
            conn.Open();

            cmd.CommandText = "SELECT * FROM [" + nomePlanilha + "]";

            dataAdapter.SelectCommand = cmd;

            dataAdapter.Fill(Data_Table);
            conn.Close();

            return (Data_Table);
        }

        private static Dictionary<string, Simbolo> Palavras_Chave()
        {
            Dictionary<string, Simbolo> TabelaDeSimbolos = new Dictionary<string, Simbolo>();

            //palavras chave da linguagem MGOL
            var inicio = new Simbolo()
            {
                Lexema = "inicio",
                Token = "inicio",
                Tipo = null
            };

            var varinicio = new Simbolo()
            {
                Lexema = "varinicio",
                Token = "varinicio",
                Tipo = null
            };

            var varfim = new Simbolo()
            {
                Lexema = "varfim",
                Token = "varfim",
                Tipo = null
            };

            var escreva = new Simbolo()
            {
                Lexema = "escreva",
                Token = "escreva",
                Tipo = null
            };

            var leia = new Simbolo()
            {
                Lexema = "leia",
                Token = "leia",
                Tipo = null
            };

            var se = new Simbolo()
            {
                Lexema = "se",
                Token = "se",
                Tipo = null
            };

            var entao = new Simbolo()
            {
                Lexema = "entao",
                Token = "entao",
                Tipo = null
            };

            var fimse = new Simbolo()
            {
                Lexema = "fimse",
                Token = "fimse",
                Tipo = null
            };

            var fim = new Simbolo()
            {
                Lexema = "fim",
                Token = "fim",
                Tipo = null
            };

            var inteiro = new Simbolo()
            {
                Lexema = "inteiro",
                Token = "inteiro",
                Tipo = "int"
            };

            var lit = new Simbolo()
            {
                Lexema = "lit",
                Token = "lit",
                Tipo = "literal"
            };

            var real = new Simbolo()
            {
                Lexema = "real",
                Token = "real",
                Tipo = "double"
            };

            TabelaDeSimbolos.Add(inicio.Lexema, inicio);
            TabelaDeSimbolos.Add(varinicio.Lexema, varinicio);
            TabelaDeSimbolos.Add(varfim.Lexema, varfim);
            TabelaDeSimbolos.Add(escreva.Lexema, escreva);
            TabelaDeSimbolos.Add(leia.Lexema, leia);
            TabelaDeSimbolos.Add(se.Lexema, se);
            TabelaDeSimbolos.Add(entao.Lexema, entao);
            TabelaDeSimbolos.Add(fimse.Lexema, fimse);
            TabelaDeSimbolos.Add(fim.Lexema, fim);
            TabelaDeSimbolos.Add(inteiro.Lexema, inteiro);
            TabelaDeSimbolos.Add(lit.Lexema, lit);
            TabelaDeSimbolos.Add(real.Lexema, real);

            return (TabelaDeSimbolos);
        }

        private static Dictionary<string, int> Simbolos_Validos()
        {
            Dictionary<string, int> SimbolosValidos = new Dictionary<string, int>();

            //cria um dicionário para efetuar as transições do automato
            SimbolosValidos.Add("A", 12);  //posição do tipo de entrada na tabela de posições
            SimbolosValidos.Add("B", 12);
            SimbolosValidos.Add("C", 12);
            SimbolosValidos.Add("D", 12);
            SimbolosValidos.Add("E", 12);
            SimbolosValidos.Add("F", 12);
            SimbolosValidos.Add("G", 12);
            SimbolosValidos.Add("H", 12);
            SimbolosValidos.Add("I", 12);
            SimbolosValidos.Add("J", 12);
            SimbolosValidos.Add("K", 12);
            SimbolosValidos.Add("L", 12);
            SimbolosValidos.Add("M", 12);
            SimbolosValidos.Add("N", 12);
            SimbolosValidos.Add("O", 12);
            SimbolosValidos.Add("P", 12);
            SimbolosValidos.Add("Q", 12);
            SimbolosValidos.Add("R", 12);
            SimbolosValidos.Add("S", 12);
            SimbolosValidos.Add("T", 12);
            SimbolosValidos.Add("U", 12);
            SimbolosValidos.Add("V", 12);
            SimbolosValidos.Add("W", 12);
            SimbolosValidos.Add("X", 12);
            SimbolosValidos.Add("Y", 12);
            SimbolosValidos.Add("Z", 12);

            SimbolosValidos.Add("a", 12);
            SimbolosValidos.Add("b", 12);
            SimbolosValidos.Add("c", 12);
            SimbolosValidos.Add("d", 12);
            SimbolosValidos.Add("e", 12);
            SimbolosValidos.Add("f", 12);
            SimbolosValidos.Add("g", 12);
            SimbolosValidos.Add("h", 12);
            SimbolosValidos.Add("i", 12);
            SimbolosValidos.Add("j", 12);
            SimbolosValidos.Add("k", 12);
            SimbolosValidos.Add("l", 12);
            SimbolosValidos.Add("m", 12);
            SimbolosValidos.Add("n", 12);
            SimbolosValidos.Add("o", 12);
            SimbolosValidos.Add("p", 12);
            SimbolosValidos.Add("q", 12);
            SimbolosValidos.Add("r", 12);
            SimbolosValidos.Add("s", 12);
            SimbolosValidos.Add("t", 12);
            SimbolosValidos.Add("u", 12);
            SimbolosValidos.Add("v", 12);
            SimbolosValidos.Add("w", 12);
            SimbolosValidos.Add("x", 12);
            SimbolosValidos.Add("y", 12);
            SimbolosValidos.Add("z", 12);

            SimbolosValidos.Add("\n", 1);
            SimbolosValidos.Add("\t", 2);
            SimbolosValidos.Add("\r", 3);
            SimbolosValidos.Add("-", 5);
            SimbolosValidos.Add("+", 4);
            SimbolosValidos.Add("*", 6);
            SimbolosValidos.Add("/", 7);
            SimbolosValidos.Add(".", 10);
            SimbolosValidos.Add("\"", 11);
            SimbolosValidos.Add("_", 13);
            SimbolosValidos.Add("{", 14);
            SimbolosValidos.Add("}", 15);
            SimbolosValidos.Add("<", 16);
            SimbolosValidos.Add(">", 17);
            SimbolosValidos.Add("=", 18);
            SimbolosValidos.Add("(", 19);
            SimbolosValidos.Add(")", 20);
            SimbolosValidos.Add(";", 21);
            SimbolosValidos.Add("EOF", 22);
            SimbolosValidos.Add("ERRO", 23);
            SimbolosValidos.Add(":", 24);
            SimbolosValidos.Add("\\", 25);

            for (int i = 0; i < 10; i++)
            {
                SimbolosValidos.Add($"{i}", 8);
            }

            return (SimbolosValidos);
        }

        private static Dictionary<string, int> Simbolos_Especiais()
        {
            Dictionary<string, int> SimbolosEspeciais = new Dictionary<string, int>();

            SimbolosEspeciais.Add("{", 1);
            SimbolosEspeciais.Add("(", 2);

            return (SimbolosEspeciais);
        }

        private static Dictionary<string, int> Estados_Finais()
        {
            Dictionary<string, int> Estado_Final = new Dictionary<string, int>();

            // estados finais do automato
            Estado_Final.Add("opm", 4);
            Estado_Final.Add("literal", 3);
            Estado_Final.Add("num", 6);
            Estado_Final.Add("Comentário", 7);
            Estado_Final.Add("opr", 8);
            Estado_Final.Add("rcb", 9);
            Estado_Final.Add("ab_p", 10);
            Estado_Final.Add("fc_p", 11);
            Estado_Final.Add("pt_v", 12);
            Estado_Final.Add("fim", 13);
            Estado_Final.Add("ERRO", 14);
            Estado_Final.Add("id", 15);
            Estado_Final.Add("qG", 19);

            return (Estado_Final); // retorna o estado em qual está
        }

        private static Dictionary<string, int> Estados()
        {
            Dictionary<string, int> Estados = new Dictionary<string, int>();

            // estados comuns do automato, considerados estados não finais
            Estados.Add("q0", 0);
            Estados.Add("qA", 1);
            Estados.Add("qB", 2);
            Estados.Add("qC", 3);
            Estados.Add("qD", 16);
            Estados.Add("qE", 17);
            Estados.Add("qF", 18);
            Estados.Add("qG", 19);

            return (Estados);
        }

        private static Dictionary<int, string> Tipos_de_Erros()
        {
            Dictionary<int, string> Tipos_de_Erros = new Dictionary<int, string>();

            // descrição dos erros que são especificados na linguagem 
            Tipos_de_Erros.Add(0, "Caracter inesperado");
            Tipos_de_Erros.Add(1, "Espera fechar chave");
            Tipos_de_Erros.Add(2, "Espera fechar parênteses");
            Tipos_de_Erros.Add(3, "Espera abrir chave");
            Tipos_de_Erros.Add(4, "Espera abrir parênteses");
            Tipos_de_Erros.Add(5, "Espera fechar aspas.");

            return (Tipos_de_Erros); // retorna o tipo de que foi gerado
        }

        private static Dictionary<string, int> Fechamento()
        {
            Dictionary<string, int> Fechamento = new Dictionary<string, int>();

            Fechamento.Add("}", 3);
            Fechamento.Add(")", 4);

            return (Fechamento);
        }
    }

}