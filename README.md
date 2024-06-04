# seri&euml;le plotter
Deze plotter maakt gebruik van een ',' om waardes te seperaten  
**voorbeeld:**  
12.65, -65.87, 9.1234
  
### Refrences
**vergeet zeker niet de System.Windows.Forms en System.Management ddl's toe te voegen bij refrences**  
Deze code maakt gebruik van een file saving mechanic dat de forms ddl gebruikt om een folderdialog te openen. Anders is de standaard locatie voor de output file in de documenten folder. De Management ddl wordt gebruikt om de discription op te halen van de poorten en niet enkel de namen zoals zou moeten met de gewone IO.Ports.  


### Drivers
Wat ook zeker niet onbelangrijk is, is de drivers van de arduino of eendere welke seri&euml;le poort je wilt gebruiken. Als deze niet geïnstalleerd zoude zijn dan zal de poort niet herkend worden en kan je hem dus niet gebruiken.


### 2wayserial.ino
Dit is een dummy arduino code dat kan gebruikt worden om de code te testen

### Seriële plotter met wpf.docx
Hier in staat meer uitleg over de code.

### UML schema van de code
![alt text](https://github.com/LucaRamboer/WPFCharting/blob/master/UML%20schema.png?raw=true)