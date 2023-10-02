## API Reference 


* /Exec/ListAll
<pre>
Method: GET
Headers: {
    Content-Type: application/json
    apikey: string
}
</pre>

* /Exec/Create
<pre>
Method: POST
Headers: {
    Content-Type: application/json
    apikey: string
} 
Body {
    "studentid": string,
    "firstname": string,
    "email": string,
    "position": int
}
</pre>
