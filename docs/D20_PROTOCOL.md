# Levantamento do protocolo D20

Este documento registrará apenas informações observadas no relógio usado pelo projeto. O nome comercial D20 não identifica um único hardware ou firmware.

## Identificação do aparelho

| Campo | Valor |
| --- | --- |
| Nome anunciado por BLE | A identificar |
| Endereço ou identificador | A identificar |
| Modelo informado na embalagem | A identificar |
| Versão de firmware | A identificar |
| Aplicativo indicado pelo fabricante | A identificar |

## Serviços e características GATT

Preencher após a inspeção com nRF Connect.

| Serviço | Característica | Propriedades | Finalidade confirmada |
| --- | --- | --- | --- |
| A identificar | A identificar | READ / WRITE / NOTIFY / INDICATE | A identificar |

## Comandos confirmados

| Função | Característica | Requisição hexadecimal | Resposta | Observações |
| --- | --- | --- | --- | --- |
| A identificar | A identificar | A identificar | A identificar | A identificar |

## Procedimento de investigação

1. Fechar outros aplicativos que possam manter conexão com o relógio.
2. Registrar serviços, características e propriedades no nRF Connect.
3. Ativar notificações somente nas características identificadas para esse fim.
4. Observar uma função por vez no aplicativo original.
5. Registrar requisição, resposta, tamanho, ordem dos bytes e possível checksum.
6. Reproduzir inicialmente apenas operações de leitura sem risco.

Não enviar pacotes para características relacionadas a OTA ou firmware durante a investigação inicial.

