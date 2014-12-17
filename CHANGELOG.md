## 0.2.0 (2014-12-16)

#### Features

* Add true partial application functionality -> This feature allows for users to partially apply an expression fairly simply and, due to metaprogramming, with appropriate compile-time errors and Intellisense hints. Ex. `someExpr.Partial().Apply(3).Result` will create a new expression where the first parameter, originally an `int`, is replaced in the expression tree by a constant `3`.
