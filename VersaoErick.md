# 🎸 Checklist de Desenvolvimento: CyberChord (V2 - Rhythm Focus)

Este arquivo serve para acompanhar o progresso técnico da nova abordagem de gameplay rítmica e de defesa da base. Marque com um `[x]` as etapas concluídas.

---

## 🏗️ Passo 1: Construção Visual da Base e Posicionamento do Pulse
- [X] **1.1** Criar um novo Objeto 2D (`Square`) na hierarquia do Unity para servir de Base Defensiva.
- [X] **1.2** Alterar o nome do objeto para `BaseDefensiva` no Inspector.
- [X] **1.3** Ajustar a escala (`Scale`) vertical e horizontal da Base para parecer uma plataforma robusta no canto esquerdo.
- [X] **1.4** Posicionar o objeto do `Pulse` exatamente acima da `BaseDefensiva`.
- [X] **1.5** Organizar a hierarquia transformando o `Pulse` em um objeto filho da `BaseDefensiva`.

---

## ⚙️ Passo 2: Mudança da Lógica de Dano (Do Pulse para a Base)
- [X] **2.1** Abrir o script `GerenciadorDeRitmo` no VS Code.
- [X] **2.2** Renomear a variável `vidaDoPulse` para `vidaDaBase`.
- [X] **2.3** Atualizar as referências da barra de vida (`barraVisual.maxValue` e `barraVisual.value`) para usarem a nova variável.
- [X] **2.4** Modificar a coordenada X de colisão dos robôs na função `Update` para bater com a borda da Base e não mais com o Pulse.
- [X] **2.5** Ajustar o texto do `Debug.Log` dentro de `TomarDano()` para indicar que a Base foi atingida.

---

## 🎨 Passo 3: Criação Visual do "Braço da Guitarra" (HUD de 4 Linhas)
- [X] **3.1** Criar um objeto de fundo na parte inferior da tela para delimitar o espaço da Guitarra.
- [X] **3.2** Criar 4 linhas horizontais paralelas dentro desse espaço.
- [X] **3.3** Criar 4 esferas receptoras (alvos de impacto), posicionando uma no final de cada linha.
- [X] **3.4** Modificar a cor de cada esfera alvo para facilitar a identificação visual do jogador.
- [X] **3.5** Adicionar textos ou ícones indicando as teclas correspondentes (**H**, **J**, **K**, **L**) ao lado de cada esfera alvo.

---

## 🎲 Passo 4: Atualização do Spawner (Gerador de Notas) por Linha
- [] **4.1** Identificar o bloco de código no `GerenciadorDeRitmo` onde ocorre o `Instantiate(moldesNotas[sorteio], ...)`.
- [] **4.2** Criar uma estrutura de checagem (`switch` ou `if/else`) para ler qual robô foi sorteado (`sorteio`).
- [] **4.3** Definir 4 variáveis de altura (coordenada **Y**) diferentes, uma para cada linha da HUD da guitarra.
- [] **4.4** Substituir a posição fixa de nascimento da nota por uma nova posição que utilize a altura **Y** correspondente à sua linha.
- [] **4.5** Testar o Spawner no Unity para garantir que as notas estão nascendo alinhadas com suas respectivas linhas da HUD.

---

## 🎯 Passo 5: Refinamento do Script de Input e Hit Físico
- [] **5.1** Modificar a função `TentarAcertarFisicamente` para aceitar um parâmetro extra indicando a linha da guitarra.
- [] **5.2** Ajustar a filtragem de busca das notas na tela para que o jogo só compare a distância das notas que pertencem à linha do botão pressionado.
- [] **5.3** Validar se a nota alvo encontrada pelo código está dentro do raio de acerto (`menorDistanciaDaEsfera <= 1.2f`).
- [] **5.4** Manter a chamada da função `ObterMaisProximoDoPulse("Inimigo")` intacta para garantir que o inimigo mais perto da base seja destruído no acerto.
- [] **5.5** Fazer um teste geral de gameplay para calibrar o tempo de resposta e a sensação de impacto (feedback rítmico).