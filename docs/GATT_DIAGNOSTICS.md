# Tela de diagnóstico GATT

## Referências de interface

A tela foi baseada no fluxo de ferramentas utilizadas para investigação BLE:

- [nRF Connect for Mobile](https://github.com/NordicSemiconductor/Android-nRF-Connect): apresenta o dispositivo conectado e a hierarquia de serviços e características.
- [LightBlue](https://punchthrough.com/how-to-use-lightblue/): destaca UUIDs e propriedades disponíveis em cada característica.
- [BluetoothGatt no Android](https://developer.android.com/reference/android/bluetooth/BluetoothGatt): a descoberta é assíncrona e entrega o resultado pelo callback de serviços descobertos.

## Estrutura adotada

Cada serviço é apresentado em um cartão contendo:

- Nome conhecido ou indicação de serviço desconhecido.
- UUID completo.
- Tipo primário ou secundário.
- Características pertencentes ao serviço.

Cada característica apresenta:

- Nome conhecido ou indicação de característica desconhecida.
- UUID completo.
- Propriedades `READ`, `WRITE`, `WRITE NO RESPONSE`, `NOTIFY`, `INDICATE`, `BROADCAST`, `SIGNED WRITE` e `EXTENDED` quando disponíveis.

UUIDs padronizados frequentes, como bateria, informações do dispositivo e frequência cardíaca, recebem nomes em português. UUIDs proprietários permanecem identificados como desconhecidos até serem confirmados no relógio.

## Segurança

A tela atual executa somente descoberta. Ela não lê valores, ativa notificações, escreve bytes, solicita vínculo nem interage com características OTA ou de firmware.

## Teste sem relógio

Em Debug Android, a busca inclui o `D20 de demonstração` junto dos periféricos reais. Ao conectar esse dispositivo, a tela exibe serviços simulados de bateria e informações do dispositivo. Builds Release não incluem essa composição de desenvolvimento.
