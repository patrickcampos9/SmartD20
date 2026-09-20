# Regras do projeto SmartD20

Estas regras são obrigatórias e se aplicam a todo o repositório. Nenhuma alteração pode ser considerada concluída ou enviada ao repositório enquanto houver violação conhecida. Exceções exigem uma decisão explícita, documentada e aprovada pelo responsável pelo projeto.

## Objetivo e escopo

O SmartD20 é um aplicativo .NET MAUI, inicialmente voltado ao Android, para comunicação BLE com relógios comercializados como D20.

O projeto deve permanecer simples e compreensível. Não introduza frameworks, camadas, bancos de dados ou serviços externos sem uma necessidade concreta do produto.

## Arquitetura

Mantenha a separação MVC adotada pelo projeto:

- `Models` contém dados e estado, sem dependência da interface.
- `Views` contém XAML e code-behind limitado à interface e ao encaminhamento de eventos.
- `Controllers` coordena casos de uso e atualiza os Models.
- `Services` contém contratos e integrações, incluindo Bluetooth e simulações.
- `Platforms/Android` contém somente código que realmente depende de APIs Android.

As Views não devem acessar Bluetooth, armazenamento ou APIs de plataforma diretamente. Controllers não devem conhecer controles visuais. Integrações devem ser expostas por interfaces e registradas em `MauiProgram`.

Não substitua MVC por MVVM, mensageria ou outra arquitetura sem uma decisão explícita do projeto.

### SOLID e extensibilidade

A arquitetura deve permanecer simples, mas todo código novo deve seguir SOLID:

- **Responsabilidade única:** cada classe e método deve ter um motivo claro para mudar.
- **Aberto/fechado:** novos relógios e protocolos devem ser adicionados por novas implementações, sem alterar fluxos estáveis desnecessariamente.
- **Substituição de Liskov:** implementações reais e simuladas devem respeitar integralmente os contratos que implementam.
- **Segregação de interfaces:** contratos devem ser pequenos e específicos; não crie interfaces extensas que obriguem implementações a depender de operações que não usam.
- **Inversão de dependência:** Controllers devem depender de abstrações, e implementações concretas devem ser conectadas pela injeção de dependências.

O nome D20 não deve ficar incorporado em componentes genéricos de transporte, descoberta ou persistência. Detalhes específicos de cada família de relógio devem ficar em implementações próprias, permitindo que outros modelos sejam adicionados no futuro.

### Reutilização e ausência de duplicidade

- Não duplique regras de negócio, validações, conversões, constantes, comandos de protocolo ou fluxos de tratamento de erro.
- Ao identificar comportamento repetido, extraia uma abstração pequena e bem nomeada no nível adequado.
- Não crie uma abstração apenas por semelhança visual ou coincidência momentânea; a extração deve representar o mesmo conceito e comportamento.
- Antes de concluir uma mudança, pesquise implementações equivalentes e elimine duplicações introduzidas ou encontradas na área alterada.

## Bluetooth e protocolo D20

- Preserve `ID20ConnectionService` como limite entre aplicação e transporte.
- Mantenha uma implementação simulada utilizável enquanto o hardware não estiver disponível.
- Coloque a montagem e interpretação de pacotes em C# puro, separada do transporte BLE.
- Execute operações GATT de forma sequencial e aceite `CancellationToken` em operações assíncronas.
- Trate desconexão, permissão negada, Bluetooth desligado, timeout e dispositivo indisponível como estados normais do fluxo.
- Nunca considere UUIDs, comandos ou formatos de pacote confirmados apenas porque foram encontrados em outro modelo vendido como D20.
- Registre em `docs/D20_PROTOCOL.md` somente dados observados no aparelho usado pelo projeto.
- Não escreva em características relacionadas a OTA, firmware ou bootloader durante a investigação do protocolo.
- Não registre chaves, tokens, endereços privados completos ou dados pessoais em logs persistentes.

## C# e operações assíncronas

- Mantenha nullable reference types habilitado e resolva os avisos na origem.
- Use nomes descritivos e tipos pequenos, com uma responsabilidade clara.
- Prefira `async Task`; use `async void` apenas em manipuladores de eventos da interface.
- Não bloqueie tarefas assíncronas com `.Wait()` ou `.Result`.
- Propague cancelamento quando a operação puder demorar ou depender de hardware.
- Capture exceções no limite adequado e apresente mensagens compreensíveis ao usuário.
- Não deixe blocos `catch` vazios.

## Interface

- Textos apresentados ao usuário devem permanecer em português do Brasil enquanto não houver sistema de localização.
- Toda operação demorada deve indicar atividade e impedir comandos concorrentes incompatíveis.
- Estados vazios, erros, conexão e desconexão devem ser visíveis na tela.
- Preserve suporte aos temas claro e escuro.
- Não exponha detalhes internos, stack traces ou códigos técnicos sem uma explicação amigável.

## Dependências e dados

- Prefira APIs da plataforma e dependências já existentes.
- Antes de adicionar um pacote, documente a finalidade e verifique manutenção, licença e compatibilidade com .NET MAUI 10.
- Não adicione SQLite, API ou nuvem enquanto o aplicativo trabalhar apenas com dados simulados ou em tempo real.
- Nunca envie segredos, certificados, keystores ou configurações locais ao Git.

## Compilação e validação

### Testes unitários obrigatórios

- Todo método criado deve ter testes unitários que comprovem seu comportamento.
- Todo método alterado deve ter seus testes atualizados ou ampliados para cobrir a mudança.
- Os testes devem cobrir o caminho esperado, limites, falhas e cancelamento quando aplicáveis.
- Métodos de interface, ciclo de vida ou integração de plataforma devem apenas encaminhar chamadas. A lógica deve ser extraída para uma classe testável e coberta por testes unitários.
- Código que acessa BLE, relógio, armazenamento ou APIs Android deve depender de abstrações que permitam testes com dublês.
- Correções de defeitos devem incluir um teste que falhe sem a correção.
- Não aceite testes que apenas repitam a implementação ou verifiquem propriedades triviais sem comportamento.

Todos os testes unitários devem passar antes do commit. Se ainda não existir um projeto de testes adequado, ele deve ser criado como parte da primeira alteração de código que exigir novos métodos.

Para mudanças comuns, valide pelo menos o destino Android:

```powershell
dotnet build D20Mobile/D20Mobile.csproj -f net10.0-android
```

Quando a mudança afetar código compartilhado ou XAML, valide também Windows quando o ambiente permitir:

```powershell
dotnet build D20Mobile/D20Mobile.csproj -f net10.0-windows10.0.19041.0
```

O Fast Deployment está desativado no Debug Android por causa do erro `XA0129` observado em dispositivo físico. Não remova `EmbedAssembliesIntoApk` sem validar repetidas implantações no aparelho.

Codec de protocolo, checksum, fragmentação e transformação de medições exigem cobertura completa dos casos conhecidos e inválidos.

## Documentação e Git

- Atualize o README quando requisitos, execução, estado do projeto ou fluxo principal mudarem.
- Atualize `docs/ARCHITECTURE.md` quando responsabilidades ou dependências mudarem.
- Atualize `docs/D20_PROTOCOL.md` com a evidência obtida no hardware.
- Não versione `.vs`, `bin`, `obj`, APKs, credenciais ou arquivos específicos da máquina.
- Faça commits pequenos e com mensagem que descreva o resultado entregue.

## Critério de conclusão

Uma mudança está concluída quando:

1. Respeita MVC, SOLID e os limites de plataforma.
2. Não introduz duplicidade de código ou regras.
3. Possui testes unitários para todos os métodos criados ou alterados.
4. Todos os testes passam.
5. Compila nos destinos afetados.
6. Trata os estados de erro relevantes.
7. Mantém o simulador funcional quando não depende exclusivamente de hardware.
8. Atualiza a documentação afetada.
9. Não inclui artefatos gerados ou informações sensíveis.
