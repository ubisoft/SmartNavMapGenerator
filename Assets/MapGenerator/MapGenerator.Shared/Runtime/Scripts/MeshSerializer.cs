﻿using System;
using System.IO;
using UnityEngine;

namespace LaForge.MapGenerator
{
    public class MeshSerializer
    {
        // A simple mesh saving/loading functionality.
        // This is a utility script, you don't need to add it to any objects.
        // See SaveMeshForWeb and LoadMeshFromWeb for example of use.
        //
        // Uses a custom binary format:
        //
        //    2 bytes vertex count
        //    2 bytes triangle count
        //    1 bytes vertex format (bits: 0=vertices, 1=normals, 2=tangents, 3=uvs)
        //
        //    After that come vertex component arrays, each optional except for positions.
        //    Which ones are present depends on vertex format:
        //        Positions
        //            Bounding box is before the array (xmin,xmax,ymin,ymax,zmin,zmax)
        //            Then each vertex component is 2 byte unsigned short, interpolated between the bound axis
        //        Normals
        //            One byte per component
        //        Tangents
        //            One byte per component
        //        UVs (8 bytes/vertex - 2 floats)
        //            Bounding box is before the array (xmin,xmax,ymin,ymax)
        //            Then each UV component is 2 byte unsigned short, interpolated between the bound axis
        //
        //    Finally the triangle indices array: 6 bytes per triangle (3 unsigned short indices)
        // Reads mesh from an array of bytes. [old: Can return null if the bytes seem invalid.]
        public static Mesh ReadMesh(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 5)
                throw new Exception("Invalid mesh file!");

            var buf = new BinaryReader(new MemoryStream(bytes));

            // read header
            var vertCount = buf.ReadUInt32();
            var triCount = buf.ReadUInt32();
            var format = buf.ReadByte();

            // sanity check
            if (vertCount < 0 || vertCount > UInt32.MaxValue)
                throw new Exception("Invalid vertex count in the mesh data!");
            if (triCount < 0 || triCount > UInt32.MaxValue)
                throw new Exception("Invalid triangle count in the mesh data!");
            if (format < 1 || (format & 1) == 0 || format > 31)
                throw new Exception("Invalid vertex format in the mesh data!");

            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            int i;

            // positions
            var verts = new Vector3[vertCount];
            ReadVector3ArrayFloat(verts, buf);
            mesh.vertices = verts;

            if ((format & 2) != 0) // have normals
            {
                var normals = new Vector3[vertCount];
                ReadVector3ArrayFloat(normals, buf);
                mesh.normals = normals;
            }

            if ((format & 4) != 0) // have tangents
            {
                var tangents = new Vector4[vertCount];
                ReadVector4ArrayBytes(tangents, buf);
                mesh.tangents = tangents;
            }

            if ((format & 8) != 0) // have UVs
            {
                var uvs = new Vector2[vertCount];
                ReadVector2Array32Bit(uvs, buf);
                mesh.uv = uvs;
            }

            if ((format & 16) != 0) // have second channel UVs
            {
                var uvs = new Vector2[vertCount];
                ReadVector2Array32Bit(uvs, buf);
                mesh.uv2 = uvs;
            }

            // triangle indices
            var tris = new int[triCount * 3];
            for (i = 0; i < triCount; ++i)
            {
                tris[i * 3 + 0] = buf.ReadInt32();
                tris[i * 3 + 1] = buf.ReadInt32();
                tris[i * 3 + 2] = buf.ReadInt32();
            }
            mesh.triangles = tris;

            buf.Close();

            return mesh;
        }

        static void ReadVector3ArrayFloat(Vector3[] arr, BinaryReader buf)
        {
            var n = arr.Length;
            if (n == 0)
                return;

            // read bounding box
            Vector3 bmin;
            Vector3 bmax;
            bmin.x = buf.ReadSingle();
            bmax.x = buf.ReadSingle();
            bmin.y = buf.ReadSingle();
            bmax.y = buf.ReadSingle();
            bmin.z = buf.ReadSingle();
            bmax.z = buf.ReadSingle();

            // decode vectors as 16 bit integer components between the bounds
            for (var i = 0; i < n; ++i)
            {
                float ix = buf.ReadSingle();
                float iy = buf.ReadSingle();
                float iz = buf.ReadSingle();
                arr[i] = new Vector3(ix, iy, iz);
            }
        }
        static void WriteVector3Array32Bit(Vector3[] arr, BinaryWriter buf)
        {
            if (arr.Length == 0)
                return;

            // calculate bounding box of the array
            var bounds = new Bounds(arr[0], new Vector3(0.001f, 0.001f, 0.001f));
            foreach (var v in arr)
                bounds.Encapsulate(v);

            // write bounds to stream
            var bmin = bounds.min;
            var bmax = bounds.max;
            buf.Write(bmin.x);
            buf.Write(bmax.x);
            buf.Write(bmin.y);
            buf.Write(bmax.y);
            buf.Write(bmin.z);
            buf.Write(bmax.z);

            // encode vectors as 16 bit integer components between the bounds
            foreach (var v in arr)
            {
                buf.Write(v.x);
                buf.Write(v.y);
                buf.Write(v.z);
            }
        }
        static void ReadVector2Array32Bit(Vector2[] arr, BinaryReader buf)
        {
            var n = arr.Length;
            if (n == 0)
                return;

            // Decode vectors as 16 bit integer components between the bounds
            for (var i = 0; i < n; ++i)
            {
                float ix = buf.ReadSingle();
                float iy = buf.ReadSingle();
                arr[i] = new Vector2(ix, iy);
            }
        }
        static void WriteVector2Array32Bit(Vector2[] arr, BinaryWriter buf)
        {
            if (arr.Length == 0)
                return;

            // Encode vectors as 16 bit integer components between the bounds
            foreach (var v in arr)
            {
                buf.Write(v.x);
                buf.Write(v.y);
            }
        }

        static void ReadVector3ArrayBytes(Vector3[] arr, BinaryReader buf)
        {
            // decode vectors as 8 bit integers components in -1.0f .. 1.0f range
            var n = arr.Length;
            for (var i = 0; i < n; ++i)
            {
                byte ix = buf.ReadByte();
                byte iy = buf.ReadByte();
                byte iz = buf.ReadByte();
                float xx = (ix - 128.0f) / 127.0f;
                float yy = (iy - 128.0f) / 127.0f;
                float zz = (iz - 128.0f) / 127.0f;
                arr[i] = new Vector3(xx, yy, zz);
            }
        }
        static void WriteVector3ArrayBytes(Vector3[] arr, BinaryWriter buf)
        {
            // encode vectors as 8 bit integers components in -1.0f .. 1.0f range
            foreach (var v in arr)
            {
                var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
                var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
                var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
                buf.Write(ix);
                buf.Write(iy);
                buf.Write(iz);
            }
        }

        static void ReadVector4ArrayBytes(Vector4[] arr, BinaryReader buf)
        {
            // Decode vectors as 8 bit integers components in -1.0f .. 1.0f range
            var n = arr.Length;
            for (var i = 0; i < n; ++i)
            {
                byte ix = buf.ReadByte();
                byte iy = buf.ReadByte();
                byte iz = buf.ReadByte();
                byte iw = buf.ReadByte();
                float xx = (ix - 128.0f) / 127.0f;
                float yy = (iy - 128.0f) / 127.0f;
                float zz = (iz - 128.0f) / 127.0f;
                float ww = (iw - 128.0f) / 127.0f;
                arr[i] = new Vector4(xx, yy, zz, ww);
            }
        }
        static void WriteVector4ArrayBytes(Vector4[] arr, BinaryWriter buf)
        {
            // Encode vectors as 8 bit integers components in -1.0f .. 1.0f range
            foreach (var v in arr)
            {
                var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
                var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
                var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
                var iw = (byte)Mathf.Clamp(v.w * 127.0f + 128.0f, 0.0f, 255.0f);
                buf.Write(ix);
                buf.Write(iy);
                buf.Write(iz);
                buf.Write(iw);
            }
        }

        // Writes mesh to an array of bytes.
        public static byte[] WriteMesh(Mesh mesh, bool saveTangents)
        {
            if (!mesh)
                throw new Exception("No mesh given!");

            var verts = mesh.vertices;
            var normals = mesh.normals;
            var tangents = mesh.tangents;
            var uvs = mesh.uv;
            var uvs2 = mesh.uv2;
            var tris = mesh.triangles;

            // figure out vertex format
            byte format = 1;
            if (normals.Length > 0)
                format |= 2;
            if (saveTangents && tangents.Length > 0)
                format |= 4;
            if (uvs.Length > 0)
                format |= 8;
            if (uvs2.Length > 0)
                format |= 16;

            var stream = new MemoryStream();
            var buf = new BinaryWriter(stream);

            // write header
            var vertCount = (UInt32)verts.Length;
            var triCount = (UInt32)(tris.Length / 3);
            buf.Write(vertCount);
            buf.Write(triCount);
            buf.Write(format);
            // vertex components
            WriteVector3Array32Bit(verts, buf);
            WriteVector3Array32Bit(normals, buf);
            if (saveTangents)
                WriteVector4ArrayBytes(tangents, buf);
            WriteVector2Array32Bit(uvs, buf);
            WriteVector2Array32Bit(uvs2, buf);
            // triangle indices
            foreach (var idx in tris)
            {
                var idx16 = idx;
                buf.Write(idx);
            }
            buf.Close();

            return stream.ToArray();
        }
    }
}
