""" VARIÁVEIS GLOBAIS """
contColuna, contLinha, ponteiroArquivo, temp = 0, 0, 0, 0 # contador coluna, contador linha e ponteiro do arquivo
aColuna, aLinha = 0, 0
atributos = {'Token': None, 'Lexema': '', 'Tipo': None, 'Erro': False, 'TipoErro': None} #Conter Token, Lexema, Tipo, Erro, TipoErro

"""---------------------------------ANALISADOR LÉXICO------------------------------------------------------------"""

""" TABELA DE TRANSIÇÃO """
#                      0    1    2    3    4    5    6    7    8    9    10   11   12   13   14   15   16   17   18   19   20   21   22
tabelaDeTransição = [[9   ,1   ,None,None,None,7   ,10  ,None,17  ,17  ,20  ,18  ,19  ,12  ,13  ,17  ,14  ,0   ,0   ,0   ,16  ,None],
                     [None,None,4   ,None,2   ,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,3   ,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,3   ,4   ,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,6   ,None,None,None,None,None,None,5   ,5   ,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,6   ,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,6   ,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [7   ,7   ,7   ,7   ,7   ,8   ,7   ,7   ,7   ,7   ,7   ,7   ,7   ,None,7   ,7   ,7   ,7   ,7   ,7   ,7   ,7   ],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [9   ,9   ,None,9   ,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [10  ,10  ,10  ,10  ,10  ,10  ,10  ,11  ,10  ,10  ,10  ,10  ,10  ,None,10  ,10  ,10  ,10  ,10  ,10  ,10  ,10  ],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,16  ,None],
                     [None,None,None,None,None,None,None,None,None,15  ,None,None,None,None,16  ,None,None,None,None,None,16  ,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                     [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None,None]]

simbolos = {'a': 0, 'b': 0, 'c': 0, 'd': 0, 'e': 0, 'f': 0, 'g': 0, 'h': 0, 'i': 0, 'j': 0, 'k': 0, 'l': 0, 'm': 0, 'n': 0, 'o': 0, 'p': 0,
            'q': 0, 'r': 0, 's': 0, 't': 0, 'u': 0, 'v': 0, 'w': 0, 'y': 0, 'x': 0, 'z': 0, 'A': 0, 'B': 0, 'C': 0, 'D': 0, 'E': 0, 'F': 0,
            'G': 0, 'H': 0, 'I': 0, 'J': 0, 'K': 0, 'L': 0, 'M': 0, 'N': 0, 'O': 0, 'P': 0, 'Q': 0, 'R': 0, 'S': 0, 'T': 0, 'U': 0, 'V': 0,
            'W': 0, 'Y': 0, 'X': 0, 'Z': 0,
            '0': 1, '1': 1, '2': 1, '3': 1, '4': 1, '5': 1, '6': 1, '7': 1, '8': 1, '9': 1,
            '_': 3,
            '.': 4,
            '"': 5,
            '{': 6,
            '}': 7,
            '+': 8,
            '-': 9,
            ';': 10,
            '(': 11,
            ')': 12,
            'EOF': 13,
            '>': 14,
            '*': 15, '/': 15,
            '<': 16,
            '\n': 17,
            '\t': 18,
            ' ': 19,
            '=': 20,
            ':': 21, '?': 21, '!': 21, '\\': 21}

estadosFinais = {1: 'Num',
                 3: 'Num',
                 6: 'Num',
                 8: 'Literal',
                 9: 'id',
                 11: 'Comentario',
                 12: 'EOF',
                 13: 'OPR',
                 14: 'OPR',
                 16: 'OPR',
                 15: 'RCB',
                 17: 'OPM',
                 18: 'AB_P',
                 19: 'FC_P',
                 20: 'PT_V',
                 21: 'ERRO'}

tiposErros = {0: 'Inserção inválida',
              2: 'Espera de número',
              4: 'Espera de número, + ou -',
              5: 'Espera de número',
              7: 'Espera fechar literal',
              10: 'Espera fechar comentário'}

""" TABELA SIMBOlOS """
tabelaDeSimbolos = [{'Token': 'entao', 'Lexema': 'entao', 'Tipo': None},
                    {'Token': 'escreva', 'Lexema': 'escreva', 'Tipo': None},
                    {'Token': 'fimse', 'Lexema': 'fimse', 'Tipo': None},
                    {'Token': 'fim', 'Lexema': 'fim', 'Tipo': None},
                    {'Token': 'inicio', 'Lexema': 'inicio', 'Tipo': None},
                    {'Token': 'int', 'Lexema': 'int', 'Tipo': 'int'},
                    {'Token': 'leia', 'Lexema': 'leia', 'Tipo': None},
                    {'Token': 'lit', 'Lexema': 'lit', 'Tipo': 'literal'},
                    {'Token': 'real', 'Lexema': 'real', 'Tipo': 'double'},
                    {'Token': 'se', 'Lexema': 'se', 'Tipo': None},
                    {'Token': 'senao', 'Lexema': 'senao', 'Tipo': None},
                    {'Token': 'varfim', 'Lexema': 'varfim', 'Tipo': None},
                    {'Token': 'varinicio', 'Lexema': 'varinicio', 'Tipo': None}]

def pos(lexema):
    for i in range(len(tabelaDeSimbolos)):
        if(tabelaDeSimbolos[i].get('Lexema') == lexema):
            return i
    return -1

def a(lexema, esquerda, direita):
    global tabelaDeSimbolos
    if (esquerda > direita):
        return False
    else:
      m = (esquerda + direita)//2
      if (tabelaDeSimbolos[m]['Lexema']) == lexema:
          return True
      if (tabelaDeSimbolos[m]['Lexema']) < lexema:
          return buscaTabelaDeSimbolos(lexema, m+1, direita)
      else:
          return buscaTabelaDeSimbolos(lexema, esquerda, m-1)

def ordenaTabelaDeSimbolos(tamanho):
    global tabelaDeSimbolos
    for i in range(0, tamanho-1):
        minimo = i
        for j in range(i+1, tamanho):
            if (tabelaDeSimbolos[j]['Lexema']) < (tabelaDeSimbolos[minimo]['Lexema']):
                minimo = j
        x = tabelaDeSimbolos[minimo]
        tabelaDeSimbolos[minimo] = tabelaDeSimbolos[i]
        tabelaDeSimbolos[i] = x

def buscaTabelaDeSimbolos(lexema, esquerda, direita):
    global tabelaDeSimbolos
    if (esquerda > direita):
        return False
    else:
      m = (esquerda + direita)//2
      if (tabelaDeSimbolos[m]['Lexema']) == lexema:
          setAtributos(tabelaDeSimbolos[m].get('Token'),tabelaDeSimbolos[m].get('Lexema'),tabelaDeSimbolos[m].get('Tipo'),atributos.get('Erro'),atributos.get('TipoErro'))
          return True
      if (tabelaDeSimbolos[m]['Lexema']) < lexema:
          return buscaTabelaDeSimbolos(lexema, m+1, direita)
      else:
          return buscaTabelaDeSimbolos(lexema, esquerda, m-1)

def inserirTabelaDeSimbolos():
    global tabelaDeSimbolos
    condição = buscaTabelaDeSimbolos(atributos['Lexema'], 0, len(tabelaDeSimbolos)-1)
    if condição == False:
        tabelaDeSimbolos.append({'Token': atributos['Token'], 'Lexema': atributos['Lexema'], 'Tipo': atributos['Tipo']})
        ordenaTabelaDeSimbolos(len(tabelaDeSimbolos))


""" FUNÇÕES """
def ajustaColunaELinha(caracterAtual):
    global contLinha, contColuna
    if caracterAtual == '\n':
        contLinha -= 1
        contColuna = temp
    else:
        contColuna -=1

def setAtributos(token, lexema, tipo, erro, tipoErro):
    atributos['Token'] = token
    atributos['Tipo'] = tipo
    atributos['TipoErro'] = tipoErro
    atributos['Lexema'] = lexema
    atributos['Erro'] = erro

def analisadorLexico(arquivoString):
    #Variáveis
    global contColuna, contLinha, ponteiroArquivo, temp, aLinha, aColuna
    estadoAFD = 0
    lexema = ''
    setAtributos(None, '', None, False, None)
    while True:
        if ponteiroArquivo == len(arquivoString):
            setAtributos(estadosFinais.get(12), 'eof', None, True, None)
            return

        caracterAtual = arquivoString[ponteiroArquivo]

        if caracterAtual == '\n':
            temp = contColuna
            contColuna = 0
            contLinha += 1
        else:
            contColuna += 1

        if caracterAtual not in simbolos.keys() or (estadoAFD == 0 and caracterAtual == '\\'):
            setAtributos(estadosFinais.get(21), caracterAtual, None, True, 'Caracter inválido (' + caracterAtual + ')')
            return

        if (estadoAFD == 1 or estadoAFD == 3) and (caracterAtual == 'e' or caracterAtual == 'E'):
            atualizaçãoEstadoAFD = 4
        else:
            atualizaçãoEstadoAFD = tabelaDeTransição[estadoAFD][simbolos.get(caracterAtual)]

        if estadoAFD in estadosFinais.keys() and atualizaçãoEstadoAFD == None:
            setAtributos(estadosFinais[estadoAFD], lexema, None, False, None)
            if estadoAFD == 9:
                inserirTabelaDeSimbolos()
            if estadoAFD == 11:
                analisadorLexico(arquivoString)
            ajustaColunaELinha(caracterAtual)
            return
        if ponteiroArquivo+1 == len(arquivoString) and atualizaçãoEstadoAFD in estadosFinais.keys():
            setAtributos(estadosFinais[atualizaçãoEstadoAFD], lexema+caracterAtual, None, False, None)
            if estadoAFD == 9:
                inserirTabelaDeSimbolos()
            if estadoAFD == 11:
                analisadorLexico(arquivoString)
            ajustaColunaELinha(caracterAtual)
            ponteiroArquivo +=1
            return
        if estadoAFD not in estadosFinais.keys() and atualizaçãoEstadoAFD == None:
            setAtributos(estadosFinais[21], lexema, None, True, tiposErros[estadoAFD] + ' não ' + caracterAtual)
            return
        if ponteiroArquivo+1 == len(arquivoString) and atualizaçãoEstadoAFD not in estadosFinais.keys() and atualizaçãoEstadoAFD != 0:
            setAtributos(estadosFinais[21], lexema+caracterAtual, None, True, tiposErros[atualizaçãoEstadoAFD])
            ponteiroArquivo += 1
            return

        ponteiroArquivo += 1
        estadoAFD = atualizaçãoEstadoAFD
        if estadoAFD == 7 or estadoAFD == 10:
            lexema += caracterAtual
        elif caracterAtual != ' ' and caracterAtual != '\n' and caracterAtual != '\t':
            lexema += caracterAtual

"""-----------------------------------ANALISADOR SINTÁTICO----------------------------------------------------------"""

""" TABELA SINTÁTICA """
tabelaSintaticaTransição = [[1   ,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,3   ,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,4   ,None,None,None,5   ,None,6   ,None,None,7   ,12  ,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,14  ,None,None,None,5   ,None,6   ,None,None,7   ,12  ,None,None],
                            [None,None,15  ,None,None,None,5   ,None,6   ,None,None,7   ,12  ,None,None],
                            [None,None,16  ,None,None,None,5   ,None,6   ,None,None,7   ,12  ,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,19  ,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,40  ,None,41  ,None,None,42  ,12  ,39  ,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,28  ,None,None,None,25  ],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,30  ,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,32  ,34  ,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,36  ,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,40  ,None,41  ,None,None,42  ,12  ,44  ,None],
                            [None,None,None,None,None,None,40  ,None,41  ,None,None,42  ,12  ,45  ,None],
                            [None,None,None,None,None,None,40  ,None,41  ,None,None,42  ,12  ,46  ,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,48  ,49  ,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,52  ,49  ,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,54  ,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None],
                            [None,None,None,None,None,None,None,None,None,None,None,None,None,None,None]]

tabelaSintaticaAção =  [['S2','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1','E1'],
                        ['Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc','Acc'],
                        ['E2','S47','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2','E2'],
                        ['E3','E3','E3','S11','E3','E3','E3','S9','E3','S10','E3','E3','E3','E3','S13','E3','E3','E3','E3','E3','S8','E3'],
                        ['r2','r2','r2','r2','r2','r2','r2','r2','R2','R2','R2','R2','R2','R2','R2','R2','R2','R2','R2','R2','R2','R2'],
                        ['E3','E3','E3','S11','E3','E3','E3','S9','E3','S10','E3','E3','E3','E3','S13','E3','E3','E3','E3','E3','S8','E3'],
                        ['E3','E3','E3','S11','E3','E3','E3','S9','E3','S10','E3','E3','E3','E3','S13','E3','E3','E3','E3','E3','S8','E3'],
                        ['E3','E3','E3','S11','E3','E3','E3','S9','E3','S10','E3','E3','E3','E3','S13','E3','E3','E3','E3','E3','S8','E3'],
                        ['R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30','R30'],
                        ['E4','E4','E4','S17','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4','E4'],
                        ['E5','E5','E5','S23','E5','E5','S21','E5','E5','E5','E5','S22','E5','E5','E5','E5','E5','E5','E5','E5','E5','E5'],
                        ['E6','E6','E6','E6','E6','E6','E6','E6','E6','E6','E6','E6','S31','E6','E6','E6','E6','E6','E6','E6','E6','E6'],
                        ['E7','E7','E7','S11','E7','E7','E7','S9','E7','S10','E7','E7','E7','E7','S13','E7','E7','E7','E7','S43','E7','E7'],
                        ['E8','E8','E8','E8','E8','E8','E8','E8','E8','E8','E8','E8','E8','E8','E8','S24','E8','E8','E8','E8','E8','E8'],
                        ['R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10','R10'],
                        ['R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16','R16'],
                        ['R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22','R22'],
                        ['E9','E9','E9','E9','E9','E9','E9','E9','S18','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9'],
                        ['R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11','R11'],
                        ['E9','E9','E9','E9','E9','E9','E9','E9','S20','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9'],
                        ['R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12','R12'],
                        ['R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13','R13'],
                        ['R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14','R14'],
                        ['R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15','R15'],
                        ['E10','E10','E10','S37','E10','E10','E10','E10','E10','E10','E10','S38','E10','E10','E10','E10','E10','E10','E10','E10','E10','E10'],
                        ['E11','E11','E11','E11','E11','E11','E11','E11','E11','E11','E11','E11','E11','E11','E11','E11','S26','E11','E11','E11','E11','E11'],
                        ['E12','E12','E12','E12','E12','E12','E12','E12','E12','E12','E12','E12','E12','E12','E12','E12','E12','S27','E12','E12','E12','E12'],
                        ['R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24','R24'],
                        ['E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','E13','S29','E13','E13','E13'],
                        ['E10','E10','E10','S37','E10','E10','E10','E10','E10','E10','E10','S38','E10','E10','E10','E10','E10','E10','E10','E10','E10','E10'],
                        ['R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25','R25'],
                        ['E10','E10','E10','S37','E10','E10','E10','E10','E10','E10','E10','S38','E10','E10','E10','E10','E10','E10','E10','E10','E10','E10'],
                        ['E9','E9','E9','E9','E9','E9','E9','E9','S33','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9'],
                        ['R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17','R17'],
                        ['E16','E16','E16','E16','E16','E16','E16','E16','R19','E16','E16','E16','E16','S35','E16','E16','E16','E16','E16','E16','E16','E16'],
                        ['E10','E10','E10','S37','E10','E10','E10','E10','E10','E10','E10','S38','E10','E10','E10','E10','E10','E10','E10','E10','E10','E10'],
                        ['R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18','R18'],
                        ['R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20','R20'],
                        ['R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21','R21'],
                        ['R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23','R23'],
                        ['E7','E7','E7','S11','E7','E7','E7','S9','E7','S10','E7','E7','E7','E7','S13','E7','E7','E7','E7','S43','E7','E7'],
                        ['E7','E7','E7','S11','E7','E7','E7','S9','E7','S10','E7','E7','E7','E7','S13','E7','E7','E7','E7','S43','E7','E7'],
                        ['E7','E7','E7','S11','E7','E7','E7','S9','E7','S10','E7','E7','E7','E7','S13','E7','E7','E7','E7','S43','E7','E7'],
                        ['R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29','R29'],
                        ['R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26','R26'],
                        ['R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27','R27'],
                        ['R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28','R28'],
                        ['E14','E14','S50','S51','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14'],
                        ['R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3','R3'],
                        ['E14','E14','S50','S51','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14','E14'],
                        ['E9','E9','E9','E9','E9','E9','E9','E9','S53','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9'],
                        ['E15','E15','E15','E15','S55','S56','E15','E15','E15','E15','S57','E15','E15','E15','E15','E15','E15','E15','E15','E15','E15','E15'],
                        ['R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4','R4'],
                        ['R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5','R5'],
                        ['E9','E9','E9','E9','E9','E9','E9','E9','S58','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9','E9'],
                        ['R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7','R7'],
                        ['R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8','R8'],
                        ['R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9','R9'],
                        ['R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6','R6']]

especificaçãoErros =   {'E1':"Expectativa inserção ""inicio"".",
                        'E2':"Expectativa inserção ""varinicio"".",
                        'E3':"Expectativa inserção ""leia"", ""escreva"", ""id"", ""se"" e ""fim"".",
                        'E4':"Expectativa inserção ""id"".",
                        'E5':"Expectativa inserção ""literal"", ""num"" ou ""id"".",
                        'E6':"Expectativa inserção ""<-"" (atribuição).",
                        'E7':"Expectativa inserção ""leia"", ""escreva"", ""id"", ""se"" ou ""fimse"".",
                        'E8':"Expectativa inserção ""("" (abre parênteses).",
                        'E9':"Expectativa inserção "";"" (ponte e vírgula).",
                        'E10':"Expectativa inserção ""id"" ou ""num"".",
                        'E11':"Expectativa inserção "")"" (fecha parênteses).",
                        'E12':"Expectativa inserção ""entao"".",
                        'E13':"Expectativa inserção de Operadores Relacionais (<, >, <=, >=, = e <>).",
                        'E14':"Expectativa inserção ""varfim"" ou ""id"".",
                        'E15':"Expectativa inserção ""int"", ""real"" ou ""lit"".",
                        'E16':"Expectativa inserção de Operadores Aritméticos (+, -, *, /)."}

terminais = {'inicio': 0,
             'varinicio': 1,
             'varfim': 2,
             'id': 3,
             'int': 4,
             'real': 5,
             'Literal': 6,
             'leia': 7,
             'PT_V': 8,
             'escreva': 9,
             'lit': 10,
             'Num': 11,
             'RCB': 12,
             'OPM': 13,
             'se': 14,
             'AB_P': 15,
             'FC_P': 16,
             'entao': 17,
             'OPR': 18,
             'fimse': 19,
             'fim': 20,
             'EOF': 21}

naoTerminais = {'P': 0,
                'V': 1,
                'A': 2,
                'LV': 3,
                'D': 4,
                'TIPO': 5,
                'ES': 6,
                'ARG': 7,
                'CMD': 8,
                'LD': 9,
                'OPRD': 10,
                'COND': 11,
                'CABEÇALHO': 12,
                'CORPO': 13,
                'EXP_R': 14}

gLC = {2: [6, "P"],
       3: [4, "V"],
       4: [4, "LV"],
       5: [4, "LV"],
       6: [6, "D"],
       7: [2, "TIPO"],
       8: [2, "TIPO"],
       9: [2, "TIPO"],
       10: [4, "A"],
       11: [6, "ES"],
       12: [6, "ES"],
       13: [2, "ARG"],
       14: [2, "ARG"],
       15: [2, "ARG"],
       16: [4, "A"],
       17: [8, "CMD"],
       18: [6, "LD"],
       19: [2, "LD"],
       20: [2, "OPRD"],
       21: [2, "OPRD"],
       22: [4, "A"],
       23: [4, "COND"],
       24: [10, "CABEÇALHO"],
       25: [6, "EXP_R"],
       26: [4, "CORPO"],
       27: [4, "CORPO"],
       28: [4, "CORPO"],
       29: [2, "CORPO"],
       30: [2, "A"]}

""" PILHA """
class Pilha(object):
    def __init__(self):
        self.dados = []
    def empilha(self, elemento):
        self.dados.append(elemento)
    def vazia(self):
        return len(self.dados) == 0
    def topo(self):
        return self.dados[len(self.dados)-1]
    def rotinaDesempilhamento(self, quantidade):
        for n in range(0, quantidade):
            if type(self.dados[-quantidade+n]) is str:
                print(self.dados[-quantidade+n] + " ", end = "")
            if not self.vazia():
                self.dados.pop(-quantidade+n)
    def rotinaDesempilhamento2(self, quantidade):
        lista = []
        quantidade = int(quantidade / 2)
        for n in range(quantidade):
            lista.append(self.dados[-quantidade+n])
            if not self.vazia():
                self.dados.pop(-quantidade+n)
        return lista

""" ANALISADOR SINTÁTICO """
def analisadorSintatico(arquivoString):
    global contColuna, contLinha, aLinha, aColuna
    pilha = Pilha()
    pilhaSemantica = Pilha()
    pilha.empilha(0)
    analisadorLexico(arquivoString)
    while True:
        if atributos.get('Erro') == False or atributos.get('Token') == 'EOF':
            s = pilha.topo()
            tSA = tabelaSintaticaAção[s][terminais.get(atributos.get('Token'))]
            if  tSA[0] == 'S':
                pilha.empilha(atributos.get('Lexema'))
                i = pos(atributos.get('Lexema'))
                if (i != -1):
                    pilhaSemantica.empilha([tabelaDeSimbolos[i]['Token'], tabelaDeSimbolos[i]['Lexema'], tabelaDeSimbolos[i]['Tipo']])
                elif(atributos.get('Token') == 'Num'):
                    if('.' in atributos.get('Lexema')):
                        pilhaSemantica.empilha([atributos.get('Token'), atributos.get('Lexema'), 'double'])
                    else:
                        pilhaSemantica.empilha([atributos.get('Token'), atributos.get('Lexema'), 'int'])
                else:
                    pilhaSemantica.empilha([atributos.get('Token'), atributos.get('Lexema'), ''])
                pilha.empilha(int(tSA.lstrip('S')))
                aLinha = contLinha
                aColuna = contColuna
                analisadorLexico(arquivoString)
            elif tSA[0] == 'R':
                listaProdução = gLC[int(tSA.lstrip('R'))]
                print(listaProdução[1] + "--> ", end = "")
                pilha.rotinaDesempilhamento(listaProdução[0])
                listaDesempilhada = pilhaSemantica.rotinaDesempilhamento2(listaProdução[0])
                print("")
                topo = pilha.topo()
                pilha.empilha(listaProdução[1])
                pilha.empilha(tabelaSintaticaTransição[topo][naoTerminais.get(listaProdução[1])])
                pilhaSemantica.empilha([listaProdução[1],'',''])
                if (regrasSemanticas(pilhaSemantica,int(tSA.lstrip('R')),listaDesempilhada)):
                    break
            elif tSA == 'Acc':
                print("P'--> P")
                analisadorSemantico()
                break;
            else:
                print("Erro: " + especificaçãoErros.get(tSA))
                print("Linha: " + str(aLinha))
                print("Coluna: " + str(aColuna))
                break;
        elif atributos.get('TipoErro') != None:
            print("Erro: " + atributos.get('TipoErro'))
            print("Linha: " + str(aLinha))
            print("Coluna: " + str(aColuna))
            break;

"""------------------------------------------------------------ANALISADOR SEMÂNTICO------------------------------------------------------------"""
texto = []
numTemporario = 0
indentação = 1

def criaIndentação(i):
    text = ''
    for j in range(i):
        text += '\t'
    return text

def regrasSemanticas(pilhaSemantica, numRegra, listaDesempilhada):
    global contColuna, contLinha, aLinha, aColuna, numTemporario, indentação
    if(numRegra == 5):
        texto.append('\n\n\n')
        return False
    elif(numRegra == 6):
        ind = criaIndentação(indentação)
        listaDesempilhada[0][2] = listaDesempilhada[1][2]
        tabelaDeSimbolos[pos(listaDesempilhada[0][1])]['Tipo'] = listaDesempilhada[0][2]
        texto.append(ind+listaDesempilhada[1][2]+' '+listaDesempilhada[0][1]+';\n')
        return False
    elif(numRegra == 7 or numRegra == 8 or numRegra == 9):
        (pilhaSemantica.topo())[2] = listaDesempilhada[0][2]
        return False
    elif(numRegra == 11):
        ind = criaIndentação(indentação)
        if(listaDesempilhada[1][2] == None):
            print('Erro 1: Variável não declarada.\nLinha: ' + str(aLinha))
            return True
        if(listaDesempilhada[1][2] == 'literal'):
            texto.append(ind+'scanf("%s", '+listaDesempilhada[1][1]+');\n')
            return False
        elif(listaDesempilhada[1][2] == 'int'):
            texto.append(ind+'scanf("%d", &'+listaDesempilhada[1][1]+');\n')
            return False
        else:
            texto.append(ind+'scanf("%lf", &'+listaDesempilhada[1][1]+');\n')
            return False
    elif(numRegra == 12):
        ind = criaIndentação(indentação)
        if (listaDesempilhada[1][0] == 'id'):
            if(listaDesempilhada[1][2] == 'int'):
                texto.append(ind+'printf("%d", ' + listaDesempilhada[1][1] + ');\n')
            elif(listaDesempilhada[1][2] == 'double'):
                texto.append(ind+'printf("%lf", ' + listaDesempilhada[1][1] + ');\n')
            else:
                texto.append(ind+'printf("%s", ' + listaDesempilhada[1][1] + ');\n')
        else:    
            texto.append(ind+'printf('+listaDesempilhada[1][1]+');\n')
        return False
    elif(numRegra == 13 or numRegra == 14):
        (pilhaSemantica.topo())[0] = listaDesempilhada[0][0]
        (pilhaSemantica.topo())[1] = listaDesempilhada[0][1]
        (pilhaSemantica.topo())[2] = listaDesempilhada[0][2]
        return False
    elif(numRegra == 15):
        if(listaDesempilhada[0][2] == None):
            print('Erro 2: Variável não declarada.\nLinha: ' + str(aLinha))
            return True
        (pilhaSemantica.topo())[0] = listaDesempilhada[0][0]
        (pilhaSemantica.topo())[1] = listaDesempilhada[0][1]
        (pilhaSemantica.topo())[2] = listaDesempilhada[0][2]
        return False
    elif(numRegra == 17):
        ind = criaIndentação(indentação)
        if(listaDesempilhada[0][2] == None):
            print('Erro 3: Variável não declarada.\nLinha: ' + str(aLinha))
            return True
        if not(listaDesempilhada[0][2] == listaDesempilhada[2][2]):
            print('Erro: Tipos diferentes para atribuição.\nLinha: ' + str(aLinha))
            return True
        texto.append(ind + listaDesempilhada[0][1]+' = '+listaDesempilhada[2][1]+';\n')
        return False
    elif(numRegra == 18):
        ind = criaIndentação(indentação)
        if not(listaDesempilhada[0][2] == listaDesempilhada[2][2] and listaDesempilhada[0][2] != 'lit'):
            print('Erro: Operandos com tipos incompatíveis.\nLinha: ' + str(aLinha))
            return True
        (pilhaSemantica.topo())[1] = 'T'+str(numTemporario)
        (pilhaSemantica.topo())[2] = listaDesempilhada[0][2]
        texto.append(ind+'T'+str(numTemporario)+' = '+listaDesempilhada[0][1]+' '+listaDesempilhada[1][1]+' '+listaDesempilhada[2][1]+';\n')
        numTemporario = numTemporario+1
        return False
    elif(numRegra == 19):
        (pilhaSemantica.topo())[0] = listaDesempilhada[0][0]
        (pilhaSemantica.topo())[1] = listaDesempilhada[0][1]
        (pilhaSemantica.topo())[2] = listaDesempilhada[0][2]
        return False
    elif(numRegra == 20):
        if(listaDesempilhada[0][2] == None):
            print('Erro 4: Variável não declarada.\nLinha: ' + str(aLinha))
            return True
        (pilhaSemantica.topo())[0] = listaDesempilhada[0][0]
        (pilhaSemantica.topo())[1] = listaDesempilhada[0][1]
        (pilhaSemantica.topo())[2] = listaDesempilhada[0][2]
        return False
    elif(numRegra == 21):
        (pilhaSemantica.topo())[0] = listaDesempilhada[0][0]
        (pilhaSemantica.topo())[1] = listaDesempilhada[0][1]
        (pilhaSemantica.topo())[2] = listaDesempilhada[0][2]
        return False
    elif(numRegra == 23):
        indentação = indentação - 1
        ind = criaIndentação(indentação)
        texto.append(ind + '}\n')
        return False
    elif(numRegra == 24):
        ind = criaIndentação(indentação)
        texto.append(ind + 'if('+listaDesempilhada[2][1]+'){\n')
        indentação = indentação + 1
        return False
    elif(numRegra == 25):
        ind = criaIndentação(indentação)
        if not(listaDesempilhada[0][2] == listaDesempilhada[2][2]):
            print('Erro: Operandos com tipos incompatíveis.\nLinha: ' + str(aLinha))
            return True
        (pilhaSemantica.topo())[1] = 'T'+str(numTemporario)
        (pilhaSemantica.topo())[2] = listaDesempilhada[0][2]
        texto.append(ind +'T'+str(numTemporario)+' = '+listaDesempilhada[0][1]+' '+listaDesempilhada[1][1]+' '+listaDesempilhada[2][1]+';\n')
        numTemporario = numTemporario+1
        return False

def analisadorSemantico():
    codigo = open('codigo.txt','w')
    codigo.write('#include<stdio.h>\ntypedef char literal[256];\n\nvoid main(void)\n{\n')
    codigo.write('\t/*----Variaveis temporarias----*/\n')
    for i in range(numTemporario):
        codigo.write('\tint T'+ str(i) + ';\n')
    codigo.write('\t/*-----------------------------*/\n')
    for i in texto:
        codigo.write(i)
    codigo.write('}')
    codigo.close()
    
    arquivoC = open('codigo.c', 'w')
    codigo = open('codigo.txt')
    for line in codigo:
        arquivoC.write(line)
    arquivoC.close()
    codigo.close()

""" MAIN """
if __name__ == '__main__':
    arquivo = input('Insira o nome do arquivo: ')

    # Abrir arquivo
    arquivo = open(arquivo)

    # Transforma o arquivo em string
    arquivoString = arquivo.read()

    analisadorSintatico(arquivoString)