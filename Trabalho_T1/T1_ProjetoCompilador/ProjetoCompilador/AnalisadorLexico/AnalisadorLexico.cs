using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace AnalisadorLexico
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

                if (_estadoAnterior == "Num" && (_caracterAtual == 'E' || _caracterAtual == 'e'))
                    _colunaDaEntrada = 9;

                _proxEstado = Convert.ToString(TabelaDeTransicao[_linhaDoEstado][_colunaDaEntrada]);                           

                if (EstadosFinais.ContainsKey(_estadoAnterior) && _proxEstado == "")
                {
                    if (_estadoAnterior == "qG")
                        _estadoAnterior = "OPR";

                    //VERIFICA SE TEM ESSE LEXEMA NA LISTA DE SIMBOLOS
                    Simbolo s = TabelaDeSimbolos.Find(o => o.Lexema == _lexema);

                    if (s == null) // s-> simbolo do codigo fonte
                    {
                        s = new Simbolo() { Lexema = _lexema, Token = _estadoAnterior, Tipo = null };

                        if (_estadoAnterior.Equals("id"))
                            TabelaDeSimbolos.Add(s); // lista de identificadores

                        s = PreencheERRO(s, _tipo_de_erro); //Verifica se ocorreu algum erro de caracter inválido durante a leitura

                        return (s);
                    }
                    else
                    {
                        s = PreencheERRO(s, _tipo_de_erro);//Verifica se ocorreu algum erro de caracter inválido durante a leitura

                        return (s);
                    }
                }

                if (!char.IsWhiteSpace(_caracterAtual) || _proxEstado == "qA") // Verifica se o caractere atual é espaço em  branco ou se o proximo estado é qA
                {
                    if (_caracterAtual == '\r') //Converte o \r para ' '
                        _caracterAtual = ' ';

                    _lexema += _caracterAtual;
                }

                _ponteiro++;
                
                if (Abertura.ContainsKey(_caracterAtual.ToString())) //Verifica há abertura de parenteses
                {
                    _contador_de_Erros++; //é um possível erro se não houver fechamento

                    Abertura.TryGetValue(_caracterAtual.ToString(), out _tipo_de_erro);

                    if (_listaDeErros.Count > 0) // Verifica se a lista está vazia
                        _listaDeErros.RemoveAt(_listaDeErros.Count - 1); //Remove o último item da lista
                    else
                        _listaDeErros.Add(CriaErro(_tipo_de_erro)); // Adiciona um erro na lista
                }

                if (fechamento.ContainsKey(_caracterAtual.ToString())) //Verifica há fechamento de parenteses
                {
                    _contador_de_Erros--;  //é um possível erro se não houver abertura

                    fechamento.TryGetValue(_caracterAtual.ToString(), out _tipo_de_erro);

                    if (_listaDeErros.Count > 0)
                        _listaDeErros.RemoveAt(_listaDeErros.Count - 1);
                    else
                        _listaDeErros.Add(CriaErro(_tipo_de_erro));
                }

                continue; // Continua o laço de análise
            }

            Simbolo fim = TabelaDeSimbolos.Find(o => o.Lexema == _lexema); // verifica se o lexema formado está na tabela de simbolo

            if (_lexema == "fim")   // verifica se chegou no final da leitura do arquivo
            {
                fim.Lexema = "EOF";
                fim.Token = "EOF";
                fim.Tipo = "EOF";
                _linha++;
            }

            if (_contador_de_Erros != 0)
            {
                foreach (Simbolo s in _listaDeErros)
                    Console.WriteLine($@"Token: {s.Token} Descrição: {s.DescricaoERRO} Linha: {s.LinhaDoERRO} Coluna: {s.ColunaDoERRO}");
            }

            return (fim);
        }

        private Simbolo CriaErro(int codErro)
        {
            //Informações necessárias para a criação de um erro
            Simbolo s = new Simbolo();

            string _descricaoERRO = "";
            s.LinhaDoERRO = ++_linha;
            s.ColunaDoERRO = _coluna;
            TiposErros.TryGetValue(codErro, out _descricaoERRO);
            s.DescricaoERRO = _descricaoERRO;

            return (s);
        }


        private Simbolo PreencheERRO(Simbolo s, int cod_erro)
        {
            if (cod_erro >= 0) //Caso ocorra um erro, as seguintes informações são solicitadas
            {
                string _descricaoERRO = "";
                s.LinhaDoERRO = ++_linha;
                s.ColunaDoERRO = _coluna;
                TiposErros.TryGetValue(cod_erro, out _descricaoERRO);
                s.DescricaoERRO = _descricaoERRO;
            }

            return (s);
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

            // 
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
                Tipo = "inteiro"
            };

            var lit = new Simbolo()
            {
                Lexema = "lit",
                Token = "lit",
                Tipo = "lit"
            };

            var real = new Simbolo()
            {
                Lexema = "real",
                Token = "real",
                Tipo = "real"
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
            Dictionary<string, int> TabelaDeSimbolos = new Dictionary<string, int>();

            //cria um dicionário para efetuar as transições do automato
            TabelaDeSimbolos.Add("A", 12);  //posição do tipo de entrada na tabela de posições
            TabelaDeSimbolos.Add("B", 12);
            TabelaDeSimbolos.Add("C", 12);
            TabelaDeSimbolos.Add("D", 12);
            TabelaDeSimbolos.Add("E", 12);
            TabelaDeSimbolos.Add("F", 12);
            TabelaDeSimbolos.Add("G", 12);
            TabelaDeSimbolos.Add("H", 12);
            TabelaDeSimbolos.Add("I", 12);
            TabelaDeSimbolos.Add("J", 12);
            TabelaDeSimbolos.Add("K", 12);
            TabelaDeSimbolos.Add("L", 12);
            TabelaDeSimbolos.Add("M", 12);
            TabelaDeSimbolos.Add("N", 12);
            TabelaDeSimbolos.Add("O", 12);
            TabelaDeSimbolos.Add("P", 12);
            TabelaDeSimbolos.Add("Q", 12);
            TabelaDeSimbolos.Add("R", 12);
            TabelaDeSimbolos.Add("S", 12);
            TabelaDeSimbolos.Add("T", 12);
            TabelaDeSimbolos.Add("U", 12);
            TabelaDeSimbolos.Add("V", 12);
            TabelaDeSimbolos.Add("W", 12);
            TabelaDeSimbolos.Add("X", 12);
            TabelaDeSimbolos.Add("Y", 12);
            TabelaDeSimbolos.Add("Z", 12);

            TabelaDeSimbolos.Add("a", 12);
            TabelaDeSimbolos.Add("b", 12);
            TabelaDeSimbolos.Add("c", 12);
            TabelaDeSimbolos.Add("d", 12);
            TabelaDeSimbolos.Add("e", 12);
            TabelaDeSimbolos.Add("f", 12);
            TabelaDeSimbolos.Add("g", 12);
            TabelaDeSimbolos.Add("h", 12);
            TabelaDeSimbolos.Add("i", 12);
            TabelaDeSimbolos.Add("j", 12);
            TabelaDeSimbolos.Add("k", 12);
            TabelaDeSimbolos.Add("l", 12);
            TabelaDeSimbolos.Add("m", 12);
            TabelaDeSimbolos.Add("n", 12);
            TabelaDeSimbolos.Add("o", 12);
            TabelaDeSimbolos.Add("p", 12);
            TabelaDeSimbolos.Add("q", 12);
            TabelaDeSimbolos.Add("r", 12);
            TabelaDeSimbolos.Add("s", 12);
            TabelaDeSimbolos.Add("t", 12);
            TabelaDeSimbolos.Add("u", 12);
            TabelaDeSimbolos.Add("v", 12);
            TabelaDeSimbolos.Add("w", 12);
            TabelaDeSimbolos.Add("x", 12);
            TabelaDeSimbolos.Add("y", 12);
            TabelaDeSimbolos.Add("z", 12);

            TabelaDeSimbolos.Add("\n", 1);
            TabelaDeSimbolos.Add("\t", 2);
            TabelaDeSimbolos.Add("\r", 3);
            TabelaDeSimbolos.Add("-", 5);
            TabelaDeSimbolos.Add("+", 4);
            TabelaDeSimbolos.Add("*", 6);
            TabelaDeSimbolos.Add("/", 7);
            TabelaDeSimbolos.Add(".", 10);
            TabelaDeSimbolos.Add("\"", 11);
            TabelaDeSimbolos.Add("_", 13);
            TabelaDeSimbolos.Add("{", 14);
            TabelaDeSimbolos.Add("}", 15);
            TabelaDeSimbolos.Add("<", 16);
            TabelaDeSimbolos.Add(">", 17);
            TabelaDeSimbolos.Add("=", 18);
            TabelaDeSimbolos.Add("(", 19);
            TabelaDeSimbolos.Add(")", 20);
            TabelaDeSimbolos.Add(";", 21);
            TabelaDeSimbolos.Add("EOF", 22);
            TabelaDeSimbolos.Add("ERRO", 23);
            TabelaDeSimbolos.Add(":", 24);
            TabelaDeSimbolos.Add("\\", 25);


            for (int i = 0; i < 10; i++)
            {
                TabelaDeSimbolos.Add($"{i}", 8);
            }

            return (TabelaDeSimbolos);
        }

        private static Dictionary<string, int> Simbolos_Especiais()
        {
            Dictionary<string, int> TabelaDeSimbolos = new Dictionary<string, int>();

            TabelaDeSimbolos.Add("{", 1);
            TabelaDeSimbolos.Add("(", 2);

            return (TabelaDeSimbolos);
        }

        private static Dictionary<string, int> Estados_Finais()
        {
            Dictionary<string, int> Estado_Final = new Dictionary<string, int>();

            // estados finais do automato
            Estado_Final.Add("OPM", 4);
            Estado_Final.Add("Literal", 3);
            Estado_Final.Add("Num", 6);
            Estado_Final.Add("Comentário", 7);
            Estado_Final.Add("OPR", 8);
            Estado_Final.Add("RCB", 9);
            Estado_Final.Add("AB_P", 10);
            Estado_Final.Add("FC_P", 11);
            Estado_Final.Add("PT_V", 12);
            Estado_Final.Add("FIM", 13);
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
            Tipos_de_Erros.Add(0, "Caracter Inválido");
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

