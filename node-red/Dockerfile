FROM nodered/node-red

WORKDIR /spend-less
COPY ./nodes .

WORKDIR /spend-less-subflows
COPY ./subflows .

WORKDIR /usr/src/node-red

RUN npm install @node-red-contrib-themes/theme-collection
RUN npm install /spend-less
RUN npm install /spend-less-subflows


