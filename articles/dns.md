---
title: DNS杂记：resolv.conf, zone, dig
---

最近因为感兴趣的东西和DNS有关，所以了解了一些零零碎碎的关于DNS的知识，并略做记录，之后又有想了解的时候再进一步补充。
目前包括的内容有：`resolv.conf`, `zone info`, `dig`.

--- MORE ---

## resolv.conf
首先是一个我之前完成不知道的，但又比较有用的文件：`/etc/resolv.conf`。
这个文件是一个通用的配置文件，一些软件都会读取它来设置一些选项，比如 dig、k8s 等。
可以通过 `man resolv.conf` 查到它的语法，大致的内容包括：
- 指定DNS使用的nameserver；
- search 允许你使用短域名进行查找，也就是省略域名的后几级，从而可以避免重复输入common的域名后缀。
ndots 决定了一个域名是先直接查找还是先append search里面的后缀。如果层级大于等于 ndots + 1，就先直接查找。
Notes: 说实话，我不太清楚这个是否真的被使用？感觉至少在用户的机器上可能不会用，对于服务器就不太清楚。
- domain 是 search 的旧版本，已不推荐使用


## Zone
Zone 就是指这个域名下的信息由某一个服务器提供 authoritative data。Zone 存在于其 parent zone 中。
Parent zone中需要保存的subzone的内容主要包括：
1. NS record, 指明了哪些服务器保存了对应zone的 authoritative data。
比如：
```bash
net.                    172800  IN      NS      a.gtld-servers.net.
net.                    172800  IN      NS      b.gtld-servers.net.
# ...
```
2. glue record, 比如上面的net我们会发现它的NS服务器属于它所在的zone中，那么访问NS服务器就需要查询 net zone 的NS服务器，形成了递归。所以这种情况中的NS服务器的地址要显式提供给parent zone，称为 glue record, 并在additional section中提供。
```bash
a.gtld-servers.net.     172800  IN      A       192.5.6.30
b.gtld-servers.net.     172800  IN      A       192.33.14.30
```

### SOA record
另外，还有一个知识点，我们看到一个zone可能有多个NS服务器，它们之间也需要一个主服务器和同步机制。虽然这个同步可以使用其他的协议来实现，但是DNS协议提供了一个[SOA机制](https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.13)。
SOA record中包括了一个 MNAME (master name) 声明了主NS服务器，和 SERIAL 一个序列号用来作为类似版本号的作用提供更新服务。其他服务器通过SOA record判断是否要更新。

## DiG
想要了解DNS的话难免需要一些debug工具，比如`nslookup`, `dig`这些，虽然目前两者共享了[核心的代码](https://github.com/isc-projects/bind9/blob/v9.19.17/bin/dig/dighost.c)，但是dig更被推荐于调试用途，所以这里大概介绍一些dig的使用和源码。

- Q: Which name server to use?
A: 可以通过命令行传入，否则默认使用 `/etc/resolv.conf`, 没有的话则fallback到localhost (127.0.0.1, ::1).
Addition notes:
  - 似乎最多只能使用3个name servers？ 见 [resconf.c](https://github.com/isc-projects/bind9/blob/v9.19.17/lib/dns/resconf.c#L74)
  - `~/.digrc` 可以从文件传入额外的参数，见 [dig.c](https://github.com/isc-projects/bind9/blob/v9.19.17/bin/dig/dig.c#L2640-L2668)

- Send request
Raw message 构造代码见 [`dns_message_render(section|end)`](https://github.com/isc-projects/bind9/blob/v9.19.17/bin/dig/dighost.c#L2627-L2634)，不细展开 

- Fallback 逻辑
`resolv.conf` search的fallback逻辑见 [dighost.c#next_origin](https://github.com/isc-projects/bind9/blob/v9.19.17/bin/dig/dighost.c#L2008)

### Outputs
其实一开始想写这篇文章的原因就是被dig的输出有些吓到，毕竟乍一看有点复杂的吓人。不过仔细一了解就发现其实就是简单的 request + response 包的文字显示，只不过它本身使用的 `;;` 提示符比较不那么清晰。

几个Notes:
1. 默认只显示response包，`+qr`可以显示request包。
2. 大概 9.15.7 版本之后支持了 YAML 输出，通过 `+yaml` 控制，虽然 YAML 也没那么易于阅读罢了。
3. 显示TTL `+ttlid / +ttlunits (human readable)` 

'#'开头的行是我加的注释，主体逻辑的代码在 [dig.c#printmessage](https://github.com/isc-projects/bind9/blob/v9.19.17/bin/dig/dig.c#L601)

```
# printgreeting中生成, 暂时保存在cmdline里面，在printmessage里面输出
# controlled by +cmd
; <<>> DiG 9.16.1-Ubuntu <<>> +qr +stats example.com
;; global options: +cmd

# 这个和 ";; Got answers:" 是printmessage主体开始的标志
# controlled by +qr
;; Sending:

# 这些对应 DNS 包中的Header部分
# https://datatracker.ietf.org/doc/html/rfc6895#section-2
# flags中的aa很关键，它说明了这个信息是不是authoritative data
;; ->>HEADER<<- opcode: QUERY, status: NOERROR, id: 28461
;; flags: rd ad; QUERY: 1, ANSWER: 0, AUTHORITY: 0, ADDITIONAL: 1

# 和 EDNS 相关，属于 addtitional 中的一部分。
;; OPT PSEUDOSECTION:
; EDNS: version: 0, flags:; udp: 4096
; COOKIE: d9c1c77ac4e331f7
;; QUESTION SECTION:
;example.com.                   IN      A

;; QUERY SIZE: 52

;; Got answer:
;; ->>HEADER<<- opcode: QUERY, status: NOERROR, id: 54046
;; flags: qr rd ad; QUERY: 1, ANSWER: 1, AUTHORITY: 0, ADDITIONAL: 0
;; WARNING: recursion requested but not available

;; QUESTION SECTION:
;example.com.                   IN      A

;; ANSWER SECTION:
example.com.            0       IN      A       93.184.216.34

;; Query time: 0 msec
;; SERVER: 192.168.16.1#53(192.168.16.1)
;; WHEN: Sat Nov 11 16:44:21 CST 2023
;; MSG SIZE  rcvd: 56
```

## TODO
EDNS, NSSEARCH
