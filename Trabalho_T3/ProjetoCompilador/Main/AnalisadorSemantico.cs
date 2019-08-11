using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Main
{
    public class AnalisadorSemantico
    {
        public Stack<Simbolo> _pilhaSemantica = new Stack<Simbolo>();
        private static string _caminhoNome = ".\\obj.txt";
        private static List<string> _listaTemporarios = new List<string>();
        int countTemp = 0;

        public AnalisadorSemantico()
        {
            DeletaArquivoObj();
        }

        public Simbolo AssociaRegraSemantica(int numProducao, List<Simbolo> tabDeSimbolos, out bool _houveErro)
        {
            Simbolo s = new Simbolo();
            Simbolo simboloNaoterminal = new Simbolo();
            StreamWriter x = File.AppendText(_caminhoNome);
            Simbolo tipo;
            Simbolo arg;
            Simbolo oprd;
            Simbolo ld;

            _houveErro = false;
            var tabelaDeSimbolos = tabDeSimbolos;
            switch (numProducao)
            {
                case 5:
                    int i = 0;
                    while (i++ < 3)
                        x.WriteLine("");
                    break;
                case 6:
                    _pilhaSemantica.Pop();
                    s.Tipo = _pilhaSemantica.Pop().Tipo;
                    Simbolo simboloTopo = _pilhaSemantica.Peek();
                    simboloTopo.Tipo = s.Tipo;
                    x.WriteLine($"{simboloTopo.Tipo} {simboloTopo.Lexema};");
                    break;
                case 7:
                    s = _pilhaSemantica.Pop();
                    tipo = new Simbolo("TIPO", "TIPO", s.Tipo);
                    _pilhaSemantica.Push(tipo);
                    break;
                case 8:
                    s = _pilhaSemantica.Pop();
                    tipo = new Simbolo("TIPO", "TIPO", s.Tipo);
                    _pilhaSemantica.Push(tipo);
                    break;
                case 9:
                    s = _pilhaSemantica.Pop();
                    tipo = new Simbolo("TIPO", "TIPO", s.Tipo);
                    _pilhaSemantica.Push(tipo);
                    break;
                case 11:
                    _pilhaSemantica.Pop();
                    s = _pilhaSemantica.Peek();
                    if (s.Tipo != null)
                    {
                        switch (s.Tipo)
                        {
                            case "literal":
                                x.WriteLine($"scanf(\"%s\",{s.Lexema});");
                                break;
                            case "int":
                                x.WriteLine($"scanf(\"%d\",&{s.Lexema});");
                                break;
                            case "double":
                                x.WriteLine($"scanf(\"%lf\",&{s.Lexema});");
                                break;
                        }
                    }
                    else
                    {
                        _houveErro = true;
                        s.DescricaoERRO = "Variável não declarada.";
                    }
                    break;
                case 12:
                    _pilhaSemantica.Pop();
                    arg = _pilhaSemantica.Peek();
                    x.WriteLine($"printf({arg.Lexema});");
                    break;
                case 13:
                    s = _pilhaSemantica.Pop();
                    arg = s.CopiaAtributos();
                    _pilhaSemantica.Push(arg);
                    break;
                case 14:
                    s = _pilhaSemantica.Pop();
                    arg = s.CopiaAtributos();
                    _pilhaSemantica.Push(arg);
                    break;
                case 15:
                    s = _pilhaSemantica.Peek();
                    if (s.Tipo != null)
                    {
                        arg = s.CopiaAtributos();
                        _pilhaSemantica.Push(arg);
                    }
                    else
                    {
                        _houveErro = true;
                        s.DescricaoERRO = "Variável não declarada.";
                    }
                    break;
                case 17:
                    _pilhaSemantica.Pop();
                    Simbolo num = _pilhaSemantica.Pop();
                    Simbolo rcb = _pilhaSemantica.Pop();
                    Simbolo id = _pilhaSemantica.Pop();
                    if (id.Tipo != null)
                    {
                        if (num.Tipo == id.Tipo)
                            x.WriteLine($"{id.Lexema} {rcb.Tipo} {num.Lexema};");
                        else
                        {
                            _houveErro = true;
                            s = id.CopiaAtributos();
                            s.DescricaoERRO = "Tipos diferentes para atribuição.";
                        }
                    }
                    else
                    {
                        _houveErro = true;
                        s = id.CopiaAtributos();
                        s.DescricaoERRO = "Variável não declarada.";
                    }
                    break;
                case 18:
                    Simbolo oprd1 = _pilhaSemantica.Pop();
                    Simbolo opm = _pilhaSemantica.Pop();
                    Simbolo oprd2 = _pilhaSemantica.Pop();
                    if (oprd1.Tipo == oprd2.Tipo)
                    {
                        _listaTemporarios.Add($"T{countTemp}");
                        Simbolo LD = new Simbolo($"T{countTemp++}", "LD", oprd1.Tipo);
                        _pilhaSemantica.Push(LD);
                        x.WriteLine($"{LD.Lexema} = {oprd2.Lexema} {opm.Tipo} {oprd1.Lexema};");
                    }
                    else
                    {
                        _houveErro = true;
                        s = oprd1.CopiaAtributos();
                        s.DescricaoERRO = "Operandos com tipos incompatíveis.";
                    }
                    break;
                case 19:
                    s = _pilhaSemantica.Pop();
                    ld = s.CopiaAtributos();
                    _pilhaSemantica.Push(ld);
                    break;
                case 20:
                    s = _pilhaSemantica.Peek();
                    if (s.Tipo != null)
                    {
                        s = _pilhaSemantica.Pop();
                        oprd = s.CopiaAtributos();
                        _pilhaSemantica.Push(oprd);
                    }
                    else
                    {
                        _houveErro = true;
                        s.DescricaoERRO = "Variável não declarada.";
                    }
                    break;
                case 21:
                    s = _pilhaSemantica.Pop();
                    oprd = s.CopiaAtributos();
                    _pilhaSemantica.Push(oprd);
                    break;
                case 23:
                    x.WriteLine("}");
                    break;
                case 24:
                    _pilhaSemantica.Pop();
                    _pilhaSemantica.Pop();
                    s = _pilhaSemantica.Pop();
                    x.WriteLine($"if({s.Lexema}){{");
                    break;
                case 25:
                    Simbolo oprnd1 = _pilhaSemantica.Pop();
                    Simbolo opr = _pilhaSemantica.Pop();
                    Simbolo oprnd2 = _pilhaSemantica.Pop();
                    if (oprnd1.Tipo == oprnd2.Tipo)
                    {
                        _listaTemporarios.Add($"T{countTemp}");
                        Simbolo exp_r = new Simbolo($"T{countTemp++}", "tx", "tx");
                        x.WriteLine($"{exp_r.Lexema} = {oprnd2.Lexema} {opr.Tipo} {oprnd1.Lexema};");
                        _pilhaSemantica.Push(exp_r);
                    }
                    else
                    {
                        _houveErro = true;
                        s = oprnd1.CopiaAtributos();
                        s.DescricaoERRO = "Operandos com tipos incompatíveis.";
                    }
                    break;
            }
            x.Close();
            return s;
        }

        public void DeletaArquivoObj()
        {
            if (File.Exists(_caminhoNome))
            {
                File.Delete(_caminhoNome);
            }
        }

        public void FinalizaArquivoObj()
        {
            string textoCompleto = File.ReadAllText(_caminhoNome);
            StringBuilder stringBuilder = new StringBuilder();
            //Cabeçalho
            stringBuilder.AppendLine("#include<stdio.h>");
            stringBuilder.AppendLine("typedef char literal[256];");
            stringBuilder.AppendLine("void main(void)\n{");
            stringBuilder.AppendLine("/*-----Variaveis temporarias-----*/");
            int i = 0;
            while (i < _listaTemporarios.Count)
            {
                stringBuilder.AppendLine($"int {_listaTemporarios[i]};");
                i++;
            }
            stringBuilder.AppendLine("/*---------------------------*/");
            File.Delete(_caminhoNome);
            StreamWriter x = File.AppendText(_caminhoNome);
            x.WriteLine($"{stringBuilder}{textoCompleto}}}");
            x.Close();
        }
    }


}
