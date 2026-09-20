# Regras do projeto SmartD20

Estas regras se aplicam a todo o repositório.

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

Para mudanças comuns, valide pelo menos o destino Android:

```powershell
dotnet build D20Mobile/D20Mobile.csproj -f net10.0-android
```

Quando a mudança afetar código compartilhado ou XAML, valide também Windows quando o ambiente permitir:

```powershell
dotnet build D20Mobile/D20Mobile.csproj -f net10.0-windows10.0.19041.0
```

O Fast Deployment está desativado no Debug Android por causa do erro `XA0129` observado em dispositivo físico. Não remova `EmbedAssembliesIntoApk` sem validar repetidas implantações no aparelho.

Adicione testes quando houver lógica determinística relevante, principalmente codec de protocolo, checksum, fragmentação ou transformação de medições. Não crie testes que apenas repitam propriedades triviais.

## Documentação e Git

- Atualize o README quando requisitos, execução, estado do projeto ou fluxo principal mudarem.
- Atualize `docs/ARCHITECTURE.md` quando responsabilidades ou dependências mudarem.
- Atualize `docs/D20_PROTOCOL.md` com a evidência obtida no hardware.
- Não versione `.vs`, `bin`, `obj`, APKs, credenciais ou arquivos específicos da máquina.
- Faça commits pequenos e com mensagem que descreva o resultado entregue.

## Critério de conclusão

Uma mudança está concluída quando:

1. Respeita a separação MVC e os limites de plataforma.
2. Compila nos destinos afetados.
3. Trata os estados de erro relevantes.
4. Mantém o simulador funcional quando não depende exclusivamente de hardware.
5. Atualiza a documentação afetada.
6. Não inclui artefatos gerados ou informações sensíveis.

