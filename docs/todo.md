## Short Term
- make "are you sure" messages more informative - e.g. "are you sure you want to delete account xyz?"
- update merge transaction ui to be more similar to edit transaction ui, inc'l showing source data hashes
- add links to tags and categories and supercategories pages
- replace table-container with hiding some columns, so the action dropdown can work correctly
- look into treemaps instead of donuts


## Medium Term
- api endpoint for generating and pulling a takeout
- cleanup/refactor things, like using `Modal.razor` everywhere, same with `Tag.razor`
- get rid of all the magic strings
- more filters
  - does not contain
  - source account is mine -> maybe this can be "source_acccount_is_mine is true"
  - destination account is mine
  - source account is not mine
  - destination account is not mine

## Long term
- account default import configuration
- configurable / last used reconcile options
- consitency with the label,title and subtitle casing
- split merged transactions
- hx-try-include

## ultra long term
- complete docs
- mobile app
- extract the midas subflows to an npm package
- unit tests lol
- there's some things (e.g. reports) that use account name in places where it really should be account id
- button on fields in reports to open popup showing transactions included in that amount
