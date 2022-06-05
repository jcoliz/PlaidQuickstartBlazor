Feature: All products provide correctly-sized results

Scenario Outline: Product loads correctly-sized results
    Given user launched site
    And link flow complete
    When clicking btn-request in the <Product> endpoint
    Then a Table is returned
    And it has <Columns> columns and <Rows> rows
    And save a screenshot named "<Product>"

Examples:
| Product                   | Columns   | Rows  |
| auth                      | 4         | 2     |
| identity                  | 4         | 9     |
| assets                    | 4         | 9     |
| balance                   | 3         | 9     |
| investments               | 5         | 12    |
| investmentstransactions   | 4         | 0     |
| liabilities               | 3         | 3     |
| itemget                   | 3         | 1     |
| accountsget               | 4         | 9     |
| transactions              | 5         | 0     |