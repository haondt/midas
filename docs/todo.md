## Short Term
- make "are you sure" messages more informative - e.g. "are you sure you want to delete account xyz?"
- longer top spending / income sources / destinations in report
- all the filters
    - add transaction filter to search by "hash is none"
- update merge transaction ui to be more similar to edit transaction ui, inc'l showing source data hashes


## Medium Term
- api endpoint for generating and pulling a takeout
- cleanup/refactor things, like using `Modal.razor` everywhere, same with `Tag.razor`
- get rid of all the magic strings

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
