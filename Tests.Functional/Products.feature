Feature: All products provide correctly-sized results

Scenario: Product loads correctly-sized results
    Given user launched site
    And link flow complete
    When clicking send request for the auth product
    Then a table of results is populated
    And the table has 4 columns and 2 rows
    And save a screenshot named "auth"