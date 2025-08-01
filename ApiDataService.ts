import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Data {
    [key: string]: any;
}

export interface DataNode<T> {
    data: T;
    isLeaf: boolean;
    children: DataNode<T>[];
}

export interface IDataService<T extends Data> {
    getContent(target: T): Observable<{ files: T[]; dirs: T[] }>;
    createDir(parent: T, name: string): Observable<T>;
    rename(target: T, newName: string): Observable<T>;
    delete(target: T[]): Observable<T>;
    uploadFiles(parent: T, files: FileList): Observable<T>;
    downloadFile(target: T): Observable<T>;
    openTree(data: T): Observable<Array<DataNode<T>>>;
}

@Injectable({
    providedIn: 'root'
})
export class ApiDataService<T extends Data> implements IDataService<T> {
    constructor(private http: HttpClient, private baseUrl: string = '/api/dataservice') {}

    getContent(target: T): Observable<{ files: T[]; dirs: T[] }> {
        return this.http.post<{ files: T[]; dirs: T[] }>(`${this.baseUrl}/content`, { path: target.path });
    }

    createDir(parent: T, name: string): Observable<T> {
        return this.http.post<T>(`${this.baseUrl}/createdir`, { parent: { path: parent.path }, name });
    }

    rename(target: T, newName: string): Observable<T> {
        return this.http.post<T>(`${this.baseUrl}/rename`, { target: { path: target.path }, newName });
    }

    delete(target: T[]): Observable<T> {
        const payload = target.map(t => ({ path: t.path }));
        return this.http.post<T>(`${this.baseUrl}/delete`, payload);
    }

    uploadFiles(parent: T, files: FileList): Observable<T> {
        const formData = new FormData();
        formData.append('parent', JSON.stringify({ path: parent.path }));
        Array.from(files).forEach(file => formData.append('files', file));
        return this.http.post<T>(`${this.baseUrl}/upload`, formData);
    }

    downloadFile(target: T): Observable<T> {
        // Ajuste conforme o endpoint real de download
        return this.http.post<T>(`${this.baseUrl}/download`, { path: target.path });
    }

    openTree(data: T): Observable<Array<DataNode<T>>> {
        // Ajuste conforme o endpoint real de árvore
        return this.http.post<Array<DataNode<T>>>(`${this.baseUrl}/opentree`, { path: data.path });
    }
}
