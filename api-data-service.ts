import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Data, DataNode, DataService, IDataService } from 'ngx-explorer';

export interface ExplorerEntity extends Data {
    id: number;
    name: string;
    path: string;
    content: string;
}

@Injectable({
    providedIn: 'root'
})
export class ApiDataService extends DataService {
    private baseUrl: string = 'https://localhost:52610/api/dataservice';

    constructor(private http: HttpClient) { super(); }

    override getContent(target: ExplorerEntity): Observable<{ files: ExplorerEntity[]; dirs: ExplorerEntity[] }> {
        return this.http.post<{ files: ExplorerEntity[]; dirs: ExplorerEntity[] }>(`${this.baseUrl}/content`, { path: target.path });
    }

    override createDir(parent: ExplorerEntity, name: string): Observable<ExplorerEntity> {
        return this.http.post<ExplorerEntity>(`${this.baseUrl}/createdir`, { parent: { path: parent.path }, name });
    }

    override rename(target: ExplorerEntity, newName: string): Observable<ExplorerEntity> {
        return this.http.post<ExplorerEntity>(`${this.baseUrl}/rename`, { target: { path: target.path }, newName });
    }

    override delete(target: ExplorerEntity[]): Observable<ExplorerEntity> {
        const payload = target.map(t => ({ path: t.path }));
        return this.http.post<ExplorerEntity>(`${this.baseUrl}/delete`, payload);
    }

    override uploadFiles(parent: ExplorerEntity, files: FileList): Observable<ExplorerEntity> {
        const formData = new FormData();
        formData.append('parent', JSON.stringify({ path: parent.path }));
        Array.from(files).forEach(file => formData.append('files', file));
        return this.http.post<ExplorerEntity>(`${this.baseUrl}/upload`, formData);
    }

    override downloadFile(target: ExplorerEntity): Observable<ExplorerEntity> {
        // Ajuste conforme o endpoint real de download
        return this.http.post<ExplorerEntity>(`${this.baseUrl}/download`, { path: target.path });
    }

    override openTree(data: ExplorerEntity): Observable<Array<DataNode<ExplorerEntity>>> {
        // Ajuste conforme o endpoint real de árvore
        return this.http.post<Array<DataNode<ExplorerEntity>>>(`${this.baseUrl}/opentree`, { path: data.path });
    }

    override getName(data: ExplorerEntity): string {
        return data.name;
    }

}
