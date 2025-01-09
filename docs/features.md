# Features

- pages
  - accounts
    - list of accounts with some info
      - name
      - balance
  - transaction import
    - multi step process
      - 1. preprocessing
        - choose file
        - choose target account
        - checkbox to add timestamp tag
      - 2. view summary
        - incoming changes
          - updated account balances
          - categories to be created
          - tags to be created
        - warnings
          - # of transactions missing categories (they will be added to uncategorized category)
            - option to rerun transactions in this group
        - errors
          - critical error encountered during run
          - transactions missing mandatory fields
            - source account
            - destination account
            - amount

- functions
  - store the original data alongside the transaction so we can re-run it through node-red if needed
