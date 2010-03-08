module ``MongoDB Extensions Specification``

open FsSpec        
open Shared
open MongoDB

module ``Describe doc operation (Converting sequence of tuples to Rocument)`` =

    let ``return at least something...`` = spec {                        
        let res = doc [v "_id" "Something"
                       v "Status" "Cool"]   

        res.should_not_be_null                
    }

    let ``support inner docs`` = spec {            
        let res = doc [v "Inner" [v "_id" "InnerSomething"]]   

        let innerResult = res.["Inner"] :?> MongoDB.Driver.Document

        innerResult.should_not_be_null
    }              