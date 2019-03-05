import {Component, Inject} from "@angular/core";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-testpage',
  templateUrl: './testpage.component.html',
})

export class TestpageComponent {
  public count = 0;
  public danon : DanonMyself;
  public httpLol: HttpClient;
  public baseUr: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl:string) {
    this.httpLol = http;
    this.baseUr = baseUrl;
  }
  public IncreaseCount() {
    this.httpLol.get<DanonMyself>(this.baseUr + 'api/SampleData/GetDanonMyself').subscribe(result => {
      this.danon = result;
    }, error => console.error(error));
  }
}

interface DanonMyself {
  pidor: string;
}
