# SmartD20

Aplicativo experimental em .NET MAUI para comunicação com relógios inteligentes comercializados como D20.

O projeto está sendo desenvolvido inicialmente para Android. A primeira versão utiliza um dispositivo simulado, permitindo construir e validar a interface, o fluxo de conexão e a arquitetura antes de termos acesso ao relógio físico e ao seu protocolo Bluetooth Low Energy (BLE).

## Estado atual

- Estrutura simples baseada em MVC.
- Busca simulada de dispositivos próximos.
- Seleção, conexão e desconexão simuladas.
- Interface preparada para apresentar estado e erros de conexão.
- Injeção de dependências configurada.
- Compilação validada para Android e Windows.

Ainda não há comunicação Bluetooth real, interpretação do protocolo D20 ou armazenamento de histórico.

## Tecnologias

- .NET 10
- .NET MAUI
- C#
- XAML
- Android como plataforma inicial

## Arquitetura

O projeto utiliza uma adaptação enxuta de MVC para .NET MAUI:

```text
View (XAML e code-behind)
        ↓
Controller
        ↓
Interface de serviço
        ↓
Simulador ou transporte BLE real
```

- `Models`: dados do dispositivo e estado da tela.
- `Views`: interface e encaminhamento das ações do usuário.
- `Controllers`: coordenação dos fluxos da aplicação.
- `Services`: comunicação simulada e, futuramente, Bluetooth real.

Uma descrição mais detalhada está em [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Documentação e regras

- [AGENTS.md](AGENTS.md): regras obrigatórias de arquitetura, código, BLE, validação e documentação.
- [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md): preparação do ambiente, fluxo de trabalho e diagnóstico.
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md): responsabilidades das camadas e evolução prevista.
- [docs/D20_PROTOCOL.md](docs/D20_PROTOCOL.md): registro das descobertas feitas no hardware.

## Pré-requisitos

- Windows 10 ou 11.
- Visual Studio com a carga de trabalho de desenvolvimento .NET MAUI.
- SDK do .NET 10.
- SDK Android API 36.
- Para executar em aparelho físico: depuração USB habilitada e computador autorizado no Android.

## Como executar

1. Abra `D20Mobile.slnx` no Visual Studio.
2. Aguarde a restauração das dependências.
3. Selecione `D20Mobile` como projeto de inicialização.
4. Escolha um dispositivo Android conectado.
5. Execute com **F5**.

Também é possível validar a compilação pela linha de comando:

```powershell
dotnet build D20Mobile/D20Mobile.csproj -f net10.0-android
```

## Testando sem o relógio

Na tela inicial:

1. Toque em **Procurar**.
2. Selecione o **D20 de demonstração**.
3. Toque em **Conectar**.
4. Confira a mudança de estado e depois toque em **Desconectar**.

O serviço utilizado nesse fluxo é `SimulatedD20ConnectionService`. Quando o relógio estiver disponível, uma implementação BLE de `ID20ConnectionService` poderá substituí-lo sem alterar a tela ou o controlador.

## Implantação Android

O Fast Deployment está desativado no modo Debug Android. Em alguns dispositivos físicos, incluindo o aparelho usado durante o desenvolvimento, ele falhou ao atualizar arquivos dentro de `.__override__` com o erro `XA0129`.

Por isso, as DLLs são incorporadas ao APK de depuração. A implantação pode levar um pouco mais de tempo, mas evita arquivos desatualizados e falhas ao substituir assemblies.

## Próximas etapas

- Configurar as permissões Bluetooth exigidas pelo Android.
- Implementar busca BLE real.
- Conectar e desconectar de um dispositivo GATT.
- Exibir serviços, características e propriedades encontradas.
- Criar um registro hexadecimal das mensagens enviadas e recebidas.
- Identificar o protocolo específico do relógio com auxílio do nRF Connect.
- Implementar comandos confirmados, como consulta de bateria e sincronização.
- Adicionar SQLite somente quando houver dados reais para armazenar.

O levantamento do protocolo será mantido em [docs/D20_PROTOCOL.md](docs/D20_PROTOCOL.md).

## Observações sobre o D20

“D20” é usado como nome comercial por diferentes fabricantes e revisões de hardware. Relógios visualmente iguais podem expor serviços GATT e comandos diferentes. Nenhum UUID ou pacote deve ser tratado como confirmado antes de ser observado no dispositivo utilizado nos testes.

As medições apresentadas por esse tipo de relógio são voltadas a acompanhamento pessoal e não devem ser usadas para diagnóstico médico.
