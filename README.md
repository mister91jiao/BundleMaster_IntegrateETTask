# BundleMaster_IntegrateETTask
<br/>BundleMaster资源加载集成ETTask示例工程</br>

<br/>网站地址: https://www.unitybundlemaster.com</br>
<br/>视频教程</br>
<br/>YouTube : https://www.youtube.com/watch?v=3P7yJu01j0I</br>
<br/>B站 : https://www.bilibili.com/video/BV19341177Ek</br>
<br/>QQ讨论群: 787652036</br>
<br/>使用ET框架请下载纯代码版进行集成: https://github.com/mister91jiao/BundleMaster</br>
<br/>ETTask部分来源于ET框架(https://github.com/egametang/ET)</br>

<br/>注意事项: </br>
<br/>WebGL 平台需要加上 BMWebGL 宏，部署使用 Local 模式运行(网页直接刷新就行所以不需要更新)，注意避免正在异步加载一个资源的时候有同步加载同一个资源。</br>
<br/>Switch 平台需要加上 Nintendo_Switch 宏，理论上可以进行热更但因为政策原因所以没有对更新功能进行适配，因此部署依然需要在 Local 模式 下运行，除此之外还要加上 NintendoSDKPlugin，不然会报错，政策原因不上传这部分内容，有需要switch开发需要可以找我联系</br>

<br/>在 OriginTestAssets 文件夹下有 WebGL构建好的Demo以及 Switch运行的视频</br>

<br/>友情链接: </br>
<br/>JEngine 一款不错的客户端框架: https://github.com/JasonXuDeveloper/JEngine</br>
<br/>HybridCLR 革命性的热更新解决方案: https://github.com/focus-creative-games/hybridclr</br>
