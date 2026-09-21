# SmartD20

Aplicativo experimental em .NET MAUI para comunicação com relógios inteligentes comercializados como D20.

O projeto está sendo desenvolvido inicialmente para Android. A versão atual busca periféricos Bluetooth Low Energy (BLE) próximos e abre conexões GATT reais, permitindo validar a infraestrutura antes de termos acesso ao relógio físico e ao protocolo específico dele. Uma implementação simulada continua disponível para testes e outros destinos.

## Estado atual

- Estrutura simples baseada em MVC.
- Busca real de periféricos Bluetooth Low Energy no Android.
- Seleção, conexão e desconexão GATT no Android.
- Tela de diagnóstico com serviços, características, UUIDs e propriedades GATT.
- Implementação simulada preservada para desenvolvimento e outros destinos.
- Interface preparada para apresentar estado e erros de conexão.
- Injeção de dependências configurada.
- Núcleo independente de plataforma com cobertura unitária integral.
- Testes automáticos configurados para pushes e pull requests.
- Compilação validada para Android e Windows.

Ainda não há descoberta de serviços GATT, interpretação do protocolo D20 ou armazenamento de histórico.

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

- `SmartD20.Core/Models`: dados do dispositivo e estado da tela.
- `D20Mobile/Views`: interface e encaminhamento das ações do usuário.
- `SmartD20.Core/Controllers`: coordenação dos fluxos da aplicação.
- `SmartD20.Core/Services`: contratos, coordenação do transporte BLE e comunicação simulada.
- `D20Mobile.Tests`: testes unitários do núcleo independente de plataforma.

Uma descrição mais detalhada está em [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Documentação e regras

- [AGENTS.md](AGENTS.md): regras obrigatórias de arquitetura MVC, SOLID, testes unitários, ausência de duplicidade, BLE, validação e documentação.
- [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md): preparação do ambiente, fluxo de trabalho e diagnóstico.
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md): responsabilidades das camadas e evolução prevista.
- [docs/D20_PROTOCOL.md](docs/D20_PROTOCOL.md): registro das descobertas feitas no hardware.
- [docs/GATT_DIAGNOSTICS.md](docs/GATT_DIAGNOSTICS.md): referências e funcionamento da tela de inspeção BLE.
- [docs/QUALITY_AUDIT.md](docs/QUALITY_AUDIT.md): resultado da revisão de conformidade e cobertura.

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

Execute os testes unitários antes de cada commit:

```powershell
dotnet test D20Mobile.Tests/D20Mobile.Tests.csproj
```

## Testando sem o relógio

No Android, a tela já permite validar a infraestrutura Bluetooth com qualquer periférico BLE próximo:

1. Toque em **Procurar**.
2. Autorize o acesso a dispositivos próximos quando o Android solicitar.
3. Confira os dispositivos encontrados e a intensidade do sinal.
4. Selecione um dispositivo BLE.
5. Toque em **Conectar** para abrir uma conexão GATT.
6. Toque em **Explorar serviços GATT** para abrir o diagnóstico.
7. Confira os serviços e características encontrados.
8. Volte à tela inicial e toque em **Desconectar**.

O Android utiliza `AndroidBluetoothLowEnergyTransport` por meio de `BluetoothWatchConnectionService`. Em Debug, os resultados reais incluem o **D20 de demonstração**, permitindo abrir a tela GATT com dados conhecidos sem o relógio. Builds Release usam somente Bluetooth real. Os demais destinos continuam usando `SimulatedWatchConnectionService`.

Conectar por GATT não significa criar um vínculo permanente na lista de aparelhos pareados do Android. Muitos relógios BLE não exigem pareamento. O aplicativo só deverá solicitar vínculo quando uma operação confirmada do relógio exigir autenticação.

## Implantação Android

O Fast Deployment está desativado no modo Debug Android. Em alguns dispositivos físicos, incluindo o aparelho usado durante o desenvolvimento, ele falhou ao atualizar arquivos dentro de `.__override__` com o erro `XA0129`.

Por isso, as DLLs são incorporadas ao APK de depuração. A implantação pode levar um pouco mais de tempo, mas evita arquivos desatualizados e falhas ao substituir assemblies.

## Próximas etapas

- Criar um registro hexadecimal das mensagens enviadas e recebidas.
- Identificar o protocolo específico do relógio com auxílio do nRF Connect.
- Implementar comandos confirmados, como consulta de bateria e sincronização.
- Adicionar SQLite somente quando houver dados reais para armazenar.

O levantamento do protocolo será mantido em [docs/D20_PROTOCOL.md](docs/D20_PROTOCOL.md).

## Observações sobre o D20

“D20” é usado como nome comercial por diferentes fabricantes e revisões de hardware. Relógios visualmente iguais podem expor serviços GATT e comandos diferentes. Nenhum UUID ou pacote deve ser tratado como confirmado antes de ser observado no dispositivo utilizado nos testes.

As medições apresentadas por esse tipo de relógio são voltadas a acompanhamento pessoal e não devem ser usadas para diagnóstico médico.
