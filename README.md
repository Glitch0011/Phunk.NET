# Phunk
A simple interpreted language.

## Input

```
dynamic engine = new Phunk.Engine();

var result = engine(new string[] {
  "a = 1 + 1",
  "b = a * 2.4",
  "c = a + b"
});

Console.WriteLine($"Result: {result.RawValue}");
```

## Output

```
->       a = 1 + 1
<-               a (= [])
->               1 + 1
<-                       1 (= 1)
<-                       1 (= 1)
<-               1 + 1 (= 2)
<-       a = 1 + 1 (= 2)
->       b = a * 2.4
<-               b (= [])
->               a * 2.4
<-                       a (= 2)
<-                       2.4 (= 2.4)
<-               a * 2.4 (= 4.8)
<-       b = a * 2.4 (= 4.8)
->       c = a + b
<-               c (= [])
->               a + b
<-                       a (= 2)
<-                       b (= 4.8)
<-               a + b (= 6.8)
<-       c = a + b (= 6.8)

Result: 6.8
```
