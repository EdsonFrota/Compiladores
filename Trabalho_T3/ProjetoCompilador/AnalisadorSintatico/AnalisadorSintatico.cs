using AnalisadorLexico;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace AnalisadorSintatico
{
    public class AnalisadorSintatico
    {
        string _codigoFonte;
        private Stack<int> _pilha = new Stack<int>();
        private Dictionary<int, string[]> _producoes = Producoes();
        private Dictionary<string, string[]> _erros = Erros();
        private static DataTable _tabelaShiftReduce = CarregaTabelaShiftReduce();

        public AnalisadorSintatico(string codigoFonte)          // Instância o analisador léxico tendo como parametro o código fonte.
        {
            this._codigoFonte = codigoFonte;             // Acessa o codigo fonte 
        }

        public void AnaliseSintatica()
        {
            AnalisadorLexico.AnalisadorLexico analisadorLexico = new AnalisadorLexico.AnalisadorLexico(_codigoFonte);  //aqui é onde ele inicializa o analisador léxico
            _pilha.Push(0);                                   // Empilha o estado inicial na pilha
            int estado = 0;
            string acao = "";
            string guardaToken = "";
            Simbolo simbolo = analisadorLexico.RetornaToken();
            Stack<Simbolo> pilhaDeSimbolos = new Stack<Simbolo>();

            while (true)    // Início do procedimento de Shift - Reduce
            {               //enquanto ele não aceitar ou dar erro ele não para a execução
                if (simbolo.Token != "ERRO") //CASO NÃO SEJA RETORNADO UM ERRO DO ANALISADOR LÉXICO
                {
                    acao = _tabelaShiftReduce.Rows[estado][$"{simbolo.Token}"].ToString(); //captura a ação da tabela shift-reduce de acordo com o estado e com o simbolo

                    if (acao.Contains("S"))  //EMPILHA 
                    {
                        estado = Convert.ToInt32(acao.Substring(1));
                        _pilha.Push(estado);

                        if (pilhaDeSimbolos.Count == 0)
                        {
                            simbolo = analisadorLexico.RetornaToken(); //atualiza o simbolo com o token retornado do léxico
                        }
                        else
                        {
                            simbolo.Token = guardaToken; //atualiza o token após o tratamento do erro
                            simbolo = pilhaDeSimbolos.Pop(); //atualiza com o simbolo da pilha
                        }     
                    }
                    else
                    {
                        if (acao.Contains("R"))   // REDUZ
                        {
                            int numProducao = Convert.ToInt32(acao.Substring(1)); //captura o numero da produção se for S32 PEGA SO O 32;
                            var producao = new string[2];
                            _producoes.TryGetValue(numProducao, out producao); //Atualiza a produção De acordo com o número da ação;
                            var Simbolos = producao[1].Split(' ');
                            int numSimbolos = Simbolos.Count();
                            //DESEMPILHA QUANTIDADE DE SÍMBOLOS LADO DIREITO
                            for (int i = 0; i < numSimbolos; i++)
                                _pilha.Pop();

                            //EMPILHAR O VALOR DE [t,A] na pilha
                            estado = Convert.ToInt32(_tabelaShiftReduce.Rows[Convert.ToInt32(_pilha.Peek())][producao[0].ToString()]); //pula para o estado seguinte
                            _pilha.Push(estado);
                            Console.WriteLine($"{producao[0]} -> {producao[1]}");
                        }
                        else
                        {
                            if (acao.Contains("ACC")) // ACEITA
                            {
                                Console.WriteLine("P' -> P");
                                Console.ReadLine();
                                break;
                            }
                            else //EM CASO DE ERRO SINTÁTICO
                            {
                                if(Convert.ToInt32(acao.Substring(1)) < 16) //CASO O ERRO SEJA TRATÁVEL
                                {
                                    Simbolo s = CopiaSimbolo(simbolo);
                                    pilhaDeSimbolos.Push(s);  //cria uma nova pilha para guardar os tokens já lidos
                                    var tipoDeErro = Erro(acao, s);
                                    guardaToken = simbolo.Token; //guarda o token do simbolo atual para atualizar depois do tratamento de erro
                                    simbolo.Token = tipoDeErro[1].ToString(); //Rotina de erro
                                }
                                else //SE O ERRO NÃO FOR TRATÁVEL A COMPILAÇÃO É PAUSADA
                                {
                                    Erro(acao, simbolo);
                                    Console.ReadLine();
                                    break;
                                } 
                            }
                        }
                    }
                }
                else//EM CASO DE ERRO LÉXICO
                {
                    PrintErro(simbolo.DescricaoERRO, simbolo); //Escreve o erro na tela 
                    simbolo = analisadorLexico.RetornaToken(); //Busca o próximo token
                }
            }
            List<Simbolo> TabelaDeSimbolos = analisadorLexico.GetTabelaDeSimbolos(); //busca a tabela de símbolos
        }

        private string[] Erro(string acao, Simbolo simbolo)  //verifica qual o tipo de erro mostra na tela o erro
        {
            string[] erro;
            _erros.TryGetValue(acao, out erro);
            simbolo.Coluna = simbolo.Coluna - 1; //AJUSTE DO PONTEIRO
            PrintErro(erro[0], simbolo);
            return (erro); //retorna o erro para tratamento
        }

        private void PrintErro(string descricao, Simbolo s = null)
        {
            Console.WriteLine("\n-------ERRO-------");
            int coluna = s.Coluna > 0 ? s.Coluna - 1 : 1;
            Console.WriteLine($"Descrição: {descricao} \nLinha: {s.Linha+1}\nColuna: {coluna}\n");
            //Console.ReadLine();
        }

        private static Dictionary<string, string[]> Erros()
        {
            Dictionary<string, string[]> erros = new Dictionary<string, string[]>(); //Dicionario de erros 

            erros.Add("E1", new string[] { "ESPERADO: 'inicio'", "inicio" });
            erros.Add("E2", new string[] { "ESPERADO: 'varinicio'", "varinicio" });
            erros.Add("E3", new string[] { "ESPERADO: 'id'", "id" });
            erros.Add("E6", new string[] { "ESPERADO: '<-' (atribuição)", "rcb" });
            erros.Add("E8", new string[] { "ESPERADO: '(' abre parênteses", "AB_P" });
            erros.Add("E9", new string[] { "ESPERADO: ';' ponto e virgula", "PT_V" });
            erros.Add("E11", new string[] { "ESPERADO: ')' fecha parênteses", "FC_P" });
            erros.Add("E12", new string[] { "ESPERADO: 'entao'", "entao" });
            erros.Add("E13", new string[] { "ESPERADO:  '<' ou '>' ou '<=' ou '>=' ou '=' ou '<>'", "opr" });
            erros.Add("E15", new string[] { "ESPERADO: 'inteiro' ou 'real' ou 'literal'", "inteiro" });
            erros.Add("E16", new string[] { "ESPERADO: '+' ou '-' ou '*' ou '/'", "opm" });
            
            erros.Add("E17", new string[] { "ESPERADO: 'leia' ou 'escreva' ou 'id' ou 'se' ou 'fim'", "leia" });
            erros.Add("E18", new string[] { "ESPERADO: 'literal' ou 'num' ou 'id'", "id" });
            erros.Add("E19", new string[] { "ESPERADO: 'leia' ou 'escreva' ou 'id' ou 'se' ou 'fimse'" , "fimse"});
            erros.Add("E20", new string[] { "ESPERADO: 'id' ou 'num'", "num" });
            erros.Add("E21", new string[] { "ESPERADO: 'varfim' ou 'id'", "varfim" });

            return erros;
        }

        private static Dictionary<int, string[]> Producoes() //Produções da Gramatica 
        {
            Dictionary<int, string[]> producoes = new Dictionary<int, string[]>();
            producoes.Add(1, new string[] { "P'", "P" });
            producoes.Add(2, new string[] { "P", "inicio V A" });
            producoes.Add(3, new string[] { "V", "varinicio LV" });
            producoes.Add(4, new string[] { "LV", "D LV" });
            producoes.Add(5, new string[] { "LV", "varfim ;" });
            producoes.Add(6, new string[] { "D", "id TIPO ;" });
            producoes.Add(7, new string[] { "TIPO", "inteiro" });
            producoes.Add(8, new string[] { "TIPO", "real" });
            producoes.Add(9, new string[] { "TIPO", "lit" });
            producoes.Add(10, new string[] { "A", "ES A" });
            producoes.Add(11, new string[] { "ES", "leia id ;" });
            producoes.Add(12, new string[] { "ES", "escreva ARG ;" });
            producoes.Add(13, new string[] { "ARG", "literal" });
            producoes.Add(14, new string[] { "ARG", "num" });
            producoes.Add(15, new string[] { "ARG", "id" });
            producoes.Add(16, new string[] { "A", "CMD A" });
            producoes.Add(17, new string[] { "CMD", "id rcb LD ;" });
            producoes.Add(18, new string[] { "LD", "OPRD opm OPRD" });
            producoes.Add(19, new string[] { "LD", "OPRD" });
            producoes.Add(20, new string[] { "OPRD", "id" });
            producoes.Add(21, new string[] { "OPRD", "num" });
            producoes.Add(22, new string[] { "A", "COND A" });
            producoes.Add(23, new string[] { "COND", "CABEÇALHO CORPO" });
            producoes.Add(24, new string[] { "CABEÇALHO", "se ( EXP_R ) entao" });
            producoes.Add(25, new string[] { "EXP_R", "OPRD opr OPRD" });
            producoes.Add(26, new string[] { "CORPO", "ES CORPO" });
            producoes.Add(27, new string[] { "CORPO", "CMD CORPO" });
            producoes.Add(28, new string[] { "CORPO", "COND CORPO" });
            producoes.Add(29, new string[] { "CORPO", "fimse" });
            producoes.Add(30, new string[] { "A", "fim" });

            return (producoes);
        }

        public static DataTable CarregaTabelaShiftReduce() //método para ler a tabela de transição, arquivo em excel
        {
            string arquivoExcel = @"TabelaShiftReduce.xlsx"; // Abre o arquivo da tabela de Shift-Reduce

            DataTable Data_Table = new DataTable();

            //Pega o endereço do arquivo excel para abrir
            string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source =" + arquivoExcel + "; Extended Properties = 'Excel 8.0;HDR=YES'"; //Pega o endereço do arquivo excel para abrir
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

        private Simbolo CopiaSimbolo(Simbolo s) //copia o simbolo com o token atual para pilha
        {
            Simbolo simbolo = new Simbolo
            {
                Token = s.Token,
                Coluna = s.Coluna,
                DescricaoERRO = s.DescricaoERRO,
                Lexema = s.Lexema,
                Linha = s.Linha,
                Tipo = s.Tipo
            };

            return (simbolo);
        }
    }
}
